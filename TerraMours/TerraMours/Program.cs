using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
using TerraMours.Framework.Infrastructure.EFCore;

var builder = WebApplication.CreateBuilder(args);

//��ȡappsetting�����ļ�
IConfiguration configuration = builder.Configuration;

//��������ļ���ʵ�����
builder.Services.Configure<SysSettings>(configuration.GetSection("SysSettings"));
var sysSettings = builder.Configuration.GetSection("SysSettings").Get<SysSettings>();

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


// ���� �����Զ���֤ ���Ƕ��ⷽ��ҲҪ�Ӷ��� ���鲻��
builder.Services.AddFluentValidationAutoValidation();
//ע�� ModifyUser2 ModifyUserIntendedEffect ��Ӧ��������cs�ļ�
builder.Services.AddScoped<IValidator<SysUserReq>, SysUserReqValidator>();

//���EF Core���ݿ�
// Add services to the container.
builder.Services.AddScoped<ISysUserService, SysUserService>();
builder.Services.AddScoped<IEmailService, EmailService>();




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



//��builder.Build();ע�͵�Ȼ�� ��Ϊ  builder.AddServices(); �Զ�ע������д�ķ���miniapi�����ɣ�����ֻ�ǵ��������ǲ���Ҫʹ��caller��
//�ܼ򵥵�ֻ�ǽ�miniapi������ǰ�Ĵ�ͳ��controller����,
//var app = builder.Build();
//���masa miniapi
var app = builder.AddServices();


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

app.Run();

