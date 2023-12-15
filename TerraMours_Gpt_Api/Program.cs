//�������û���� Npgsql �ͻ����� Postgres ������֮���ʱ�����Ϊ����������ֱ���޸� Postgres ��ʱ�����á�
//ʱ������Ҫ��ǰ��֮ǰ����ݼ����ᵼ��ʧЧ
using Essensoft.Paylink.Alipay;
using Serilog.Events;
using Serilog;
using TerraMours.Framework.Infrastructure.Contracts.Commons;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.Commons;
using TerraMours.Framework.Infrastructure.Services;
using AutoMapper;
using FluentValidation.AspNetCore;
using FluentValidation;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours.Domains.LoginDomain.Contracts.ReqValidators;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Req;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;
using TerraMours.Domains.LoginDomain.Contracts.Res;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Res;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels;
using TerraMours_Gpt.Domains.PayDomain.Contracts.Req;
using Essensoft.Paylink.Alipay.Domain;
using TerraMours_Gpt.Domains.PayDomain.Contracts.Res;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.PaymentModels;
using TerraMours.Domains.LoginDomain.IServices;
using TerraMours.Domains.LoginDomain.Services;
using TerraMours_Gpt.Domains.GptDomain.IServices;
using TerraMours_Gpt.Domains.GptDomain.Services;
using TerraMours_Gpt.Domains.LoginDomain.IServices;
using TerraMours_Gpt.Domains.LoginDomain.Services;
using TerraMours_Gpt.Domains.PayDomain.IServices;
using TerraMours_Gpt.Domains.PayDomain.Services;
using TerraMours_Gpt.Framework.Infrastructure.EFCore;
using Microsoft.EntityFrameworkCore;
using TerraMours.Framework.Infrastructure.EFCore;
using Hangfire;
using TerraMours.Framework.Infrastructure.Redis;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TerraMours.Framework.Infrastructure.Filters;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.FileProviders;
using TerraMours_Gpt.Framework.Infrastructure.Middlewares;
using TerraMours_Gpt.Domains.GptDomain.Hubs;
using TerraMours_Gpt.Domains.PayDomain.Hubs;
using Microsoft.OpenApi.Models;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

var builder = WebApplication.CreateBuilder(args);
//�������
builder.Services.AddHealthChecks()
//����������Լ����Զ���Ľ�������߼� ʹ��Ĭ�ϵĿ���ע��
    .AddCheck<HealthCheckService>("HealthCheck");
builder.Services.AddHealthChecksUI().AddInMemoryStorage();

// Add services to the container.
builder.Services.AddScoped<ISysUserService, SysUserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISysRoleService, SysRoleService>();
builder.Services.AddScoped<ISysMenuService, SysMenuService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IAnalysisService, AnalysisService>();
builder.Services.AddScoped<ISeedDataService, SeedDataService>();
//gpt
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IImageService, ImageService>();

//֧����pay
builder.Services.AddScoped<IPayService, AliPayService>();

//��Ʒ����
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
//��ʼ��
builder.Services.AddTransient<DbInitialiser>();
// ���Paylink����ע��
builder.Services.AddAlipay();
//������
builder.Services.AddScoped<ExceptionFilter>();
//��ȡappsetting�����ļ�
IConfiguration configuration = builder.Configuration;

//��������ļ���ʵ�����
builder.Services.Configure<SysSettings>(configuration.GetSection("SysSettings"));
var sysSettings = builder.Configuration.GetSection("SysSettings").Get<SysSettings>() ?? throw new Exception("�û��������벻��ȷ");
var isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development;
var dbConnStr = isDev ? sysSettings.connection.DbConnectionString : (Environment.GetEnvironmentVariable("ENV_DB_CONNECTION") ?? sysSettings.connection.DbConnectionString);
var seqUrl = isDev ? sysSettings.SeqUrl : (Environment.GetEnvironmentVariable("ENV_SEQ_HOST") ?? sysSettings.SeqUrl);
Console.WriteLine("��ǰ�������ݿ⣺" + dbConnStr);
var redisConnStr = isDev ? sysSettings.connection.RedisHost : (Environment.GetEnvironmentVariable("ENV_REDIS_HOST") ?? sysSettings.connection.RedisHost);
builder.Services.Configure<GptOptions>(configuration.GetSection("GptOptions"));
builder.Services.Configure<AlipayOptions>(configuration.GetSection("Alipay"));
//���ݿ�
builder.Services.AddDbContext<FrameworkDbContext>(opt => {
    //�������ļ��л�ȡkey,���ַ�����Ҫ����һ������֮��Ӧ

    //var connStr = $"Host=localhost;Database=TerraMours;Username=postgres;Password=root";
    var connStr = dbConnStr;
    opt.UseNpgsql(connStr);
    //����EFĬ��AsNoTracking,EF Core�� ����
    opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    if (isDev) {
        //���ô�ѡ���EF Core������־�а����������ݣ�����ʵ�������ֵ������ڵ��Ժ��Ų�����ǳ����á�
        opt.EnableSensitiveDataLogging();
    }

    opt.EnableDetailedErrors();
});
// Add Hangfire services.
builder.Services.AddHangfire(config => config.UseStorage(new PostgreSqlStorage(dbConnStr)));
//redis ���� ���ʵ����IDistributedCache
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = redisConnStr;
    options.InstanceName = sysSettings.connection.RedisInstanceName;
});
builder.Services.AddScoped<IDistributedCacheHelper, DistributedCacheHelper>();
//ע����־
// ���� Serilog ��־��¼��
Console.WriteLine("��ǰ����seq��" + seqUrl);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    //.WriteTo.File()
    .WriteTo.Seq(seqUrl)
    .CreateLogger();
builder.Host.UseSerilog(Log.Logger);

//automapper
// ����ӳ�����
//��Ӻ���nullֵ�����ã�.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)
MapperConfiguration mapperConfig = new(cfg => {
    cfg.CreateMap<SysUserUpdateReq, SysUser>().ForMember(m => m.UserId, n => n.Ignore()).ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<SysUser, SysUserDetailRes>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<SysUserAddReq, SysUser>().ForMember(m => m.UserId, n => n.Ignore()).ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<SysRole, SysRoleRes>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<SysMenuReq, SysMenus>().ForMember(m => m.MenuId, n => n.Ignore()).ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<SysMenus, SysMenuRes>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<KeyOptions, KeyOptionRes>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<Sensitive, SensitiveRes>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<ChatConversation, ChatConversationRes>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<ChatRecord, ChatRes>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<ChatRes, ChatRecord>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<ImageRecord, ImageRes>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<PromptOptions, PromptOptionRes>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<ProductReq, Product>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<Product, ProductRes>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<CategoryReq, Category>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<Category, CategoryRes>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    cfg.CreateMap<Order, OrderRes>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
});
//ע������
IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

// ���� �����Զ���֤ ���Ƕ��ⷽ��ҲҪ�Ӷ��� ���鲻��
builder.Services.AddFluentValidationAutoValidation();
//ע�� ModifyUser2 ModifyUserIntendedEffect ��Ӧ��������cs�ļ�
builder.Services.AddScoped<IValidator<SysUserReq>, SysUserReqValidator>();
builder.Services.AddScoped<IValidator<SysLoginUserReq>, SysLoginUserReqValidator>();

builder.Services.AddCors(options => {
    options.AddPolicy(name: "MyPolicy",
                      policy => {
                          policy.SetIsOriginAllowed(_ => true)
                                   .AllowAnyMethod()
                                   .AllowAnyHeader()
                          .AllowCredentials();
                      });
});

builder.Services.AddControllers();

//jsonСд������
builder.Services.Configure<JsonOptions>(options => {
    //net6�� options.JsonSerializerOptions.PropertyNamingPolicy = null;

    //net7 PropertyNameCaseInsensitive = true
    //����ԭ���ֶ���
    //options.SerializerOptions.PropertyNamingPolicy = null;
    //�����ִ�Сд
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// SignalR
builder.Services.AddSignalR();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    //��¼�ɹ�֮����token,��swagger ���Ͻ���ͼ��λ������token
    //�����ʽΪBearer xxxxxx   
    //ע��Bearer������һ���ո񣬺���������token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme() {
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
    //����XML��ע�ĵ�
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TerraMours_Gpt.xml"));
});
//�����֤  ��Ȩ����
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters() {
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

var app = builder.Build();

//��ʼ�����ݿ�
using var scope = app.Services.CreateScope();

var services = scope.ServiceProvider;

var initialiser = services.GetRequiredService<DbInitialiser>();

initialiser.Run();

//�������
//app.UseHealthChecks("/health");
app.UseHealthChecksUI();
//��־
app.UseSerilogRequestLogging();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();


// Use Hangfire server and dashboard.
app.UseHangfireServer(new BackgroundJobServerOptions {
    Queues = new[] { "default", "img-queue" },
    WorkerCount = 1
});
app.UseHangfireDashboard();// ʹ�� Hangfire �������

//app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory + "/images"),
    RequestPath = ""
});

//`UseCors` ��� CORS �м���� �� `UseCors` �ĵ��ñ������ `UseRouting` ֮�󣬵��� `UseAuthorization` ֮ǰ�� ��Ȼ�����ǰ�˻�ȡ����response������
app.UseCors("MyPolicy");
//�����м��
app.UseMiddleware<KeyMiddleware>();
app.UseAuthorization();

app.MapControllers();
// SignalR hub
app.MapHub<GraphGenerationHub>("/graphhub");
app.MapHub<PaymentHub>("/Hubs/QueryPaymentStatus");
app.Run();
