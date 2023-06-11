using AutoMapper;
using Masa.BuildingBlocks.Service.MinimalAPIs;
using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours.Domains.LoginDomain.Contracts.ReqValidators;
using TerraMours.Domains.LoginDomain.IServices;
using TerraMours.Domains.LoginDomain.Services;
using TerraMours.Framework.Infrastructure.Contracts.Commons;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours.Framework.Infrastructure.Filters;
using TerraMours.Framework.Infrastructure.Redis;
using TerraMours.Domains.LoginDomain.Contracts.Res;
using TerraMours.Framework.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

//�������
builder.Services.AddHealthChecks()
//����������Լ����Զ���Ľ�������߼� ʹ��Ĭ�ϵĿ���ע��
    .AddCheck<HealthCheckService>("HealthCheck");
builder.Services.AddHealthChecksUI().AddInMemoryStorage();

//��ȡappsetting�����ļ�
IConfiguration configuration = builder.Configuration;

//��������ļ���ʵ�����
builder.Services.Configure<SysSettings>(configuration.GetSection("SysSettings"));
var sysSettings = builder.Configuration.GetSection("SysSettings").Get<SysSettings>() ?? throw new Exception("�û��������벻��ȷ");

//ע����־
// ���� Serilog ��־��¼��

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    //.WriteTo.File()
    .WriteTo.Seq("http://localhost:5341/")
    .CreateLogger();
builder.Host.UseSerilog(Log.Logger);

//minimal Service ���캯��û��Ilog �ᱨ�� 
/*builder.Services.AddLogging(builder =>
{
    Log.Logger = new LoggerConfiguration()
     // .MinimumLevel.Information().Enrich.FromLogContext()
     .MinimumLevel.Debug()
     .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
     .Enrich.FromLogContext()
     .WriteTo.Console()
     //.WriteTo.File(initOptions.LogFilePath)
     .WriteTo.Seq("http://localhost:5341/")
     .CreateLogger();
    builder.AddSerilog();
});*/


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    //��¼�ɹ�֮����token,��swagger ���Ͻ���ͼ��λ������token
    //�����ʽΪBearer xxxxxx   
    //ע��Bearer������һ���ո񣬺���������token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Description = "���¿�����������ͷ����Ҫ���Jwt��ȨToken��Bearer Token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
//automapper
// ����ӳ�����
MapperConfiguration mapperConfig = new(cfg => {
    cfg.CreateMap<SysUserDetailRes, SysUser>().ForMember(m=>m.UserId,n=>n.Ignore());
    cfg.CreateMap<SysUserAddReq, SysUser>().ForMember(m => m.UserId, n => n.Ignore());
    cfg.CreateMap<SysRole, SysRoleRes>();
    cfg.CreateMap<SysMenuReq, SysMenus>().ForMember(m => m.MenuId, n => n.Ignore());
    cfg.CreateMap<SysMenus, SysMenuRes>();
});
//ע������
IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);


// ���� �����Զ���֤ ���Ƕ��ⷽ��ҲҪ�Ӷ��� ���鲻��
builder.Services.AddFluentValidationAutoValidation();
//ע�� ModifyUser2 ModifyUserIntendedEffect ��Ӧ��������cs�ļ�
builder.Services.AddScoped<IValidator<SysUserReq>, SysUserReqValidator>();
builder.Services.AddScoped<IValidator<SysLoginUserReq>, SysLoginUserReqValidator>();


//���EF Core���ݿ�
// Add services to the container.
builder.Services.AddScoped<ISysUserService, SysUserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISysRoleService, SysRoleService>();
builder.Services.AddScoped<ISysMenuService, SysMenuService>();
builder.Services.AddCors(options => {
    options.AddPolicy(name: "MyPolicy",
                      policy => {
                          policy.AllowAnyOrigin()
                                   .AllowAnyMethod()
                                   .AllowAnyHeader();
                          //.AllowCredentials();
                      });
});

//redis ���� ���ʵ����IDistributedCache 
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = sysSettings.connection.RedisHost;
    options.InstanceName = sysSettings.connection.RedisInstanceName;
});
builder.Services.AddScoped<IDistributedCacheHelper, DistributedCacheHelper>();


//������
builder.Services.AddScoped<ExceptionFilter>();
//builder.Services.AddScoped<GlobalActionFilter>();

//�����֤  ��Ȩ����
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        //ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidIssuer = sysSettings.jwt.Issuer,
        ValidateAudience = true,
        //ValidAudience = builder.Configuration["JWT:Audience"],
        ValidAudience = sysSettings.jwt.Audience,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(sysSettings.jwt.SecretKey))
        //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
    };
});

builder.Services.AddAuthorization();


builder.Services.AddDbContext<FrameworkDbContext>(opt =>
{
    //�������ļ��л�ȡkey,���ַ�����Ҫ����һ������֮��Ӧ

    //var connStr = $"Host=localhost;Database=TerraMours;Username=postgres;Password=root";
    var connStr = sysSettings.connection.DbConnectionString;
    opt.UseNpgsql(connStr);

});

//jsonСд������
builder.Services.Configure<JsonOptions>(options =>
{
    //net6�� options.JsonSerializerOptions.PropertyNamingPolicy = null;

    //net7 PropertyNameCaseInsensitive = true
    //����ԭ���ֶ���
    //options.SerializerOptions.PropertyNamingPolicy = null;
    //�����ִ�Сд
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

//��������м��
/*var limiterName = "MyLimiterName";

var options = new RateLimiterOptions()
    .AddTokenBucketLimiter(limiterName, new TokenBucketRateLimiterOptions(1, QueueProcessingOrder.OldestFirst, 1, TimeSpan.FromSeconds(8), 1));

builder.Services.AddSingleton(options);
*/



//��builder.Build();ע�͵�Ȼ�� ��Ϊ  builder.AddServices(); �Զ�ע������д�ķ���miniapi�����ɣ�����ֻ�ǵ��������ǲ���Ҫʹ��caller��
//�ܼ򵥵�ֻ�ǽ�miniapi������ǰ�Ĵ�ͳ��controller����,
//var app = builder.Build();
//���masa miniapi
var app = builder.AddServices(opt => {
    opt.DisableAutoMapRoute = true;
});

//�������
//app.UseHealthChecks("/health");
app.UseHealthChecksUI();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//��־
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

//���jwt��֤
app.UseAuthentication();
app.UseAuthorization();

//�������û���� Npgsql �ͻ����� Postgres ������֮���ʱ�����Ϊ����������ֱ���޸� Postgres ��ʱ�����á�
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
app.UseCors("MyPolicy");


//ʹ��minimal api
app.MapHealthChecks("/health", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

////����д���ʵ�ַΪhttp://localhost:5179/health-ui#/healthchecks
//app.MapHealthChecksUI(options => options.UIPath = "/health-ui");


//����ȫ���쳣
//app.MapGet("/testError", () => { throw new Exception("�����쳣"); }).AddEndpointFilter<ExceptionFilter>(); ;

//��ʹ��minimal api
/*app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    //����д���ʵ�ַΪhttp://localhost:5179/health-ui#/healthchecks
    //endpoints.MapHealthChecksUI(options => options.UIPath = "/health-ui");
});
*/

app.Run();

