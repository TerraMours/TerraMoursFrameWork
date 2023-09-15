using AutoMapper;
using Essensoft.Paylink.Alipay;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours.Domains.LoginDomain.Contracts.ReqValidators;
using TerraMours.Domains.LoginDomain.Contracts.Res;
using TerraMours.Domains.LoginDomain.IServices;
using TerraMours.Domains.LoginDomain.Services;
using TerraMours.Framework.Infrastructure.Contracts.Commons;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours.Framework.Infrastructure.Filters;
using TerraMours.Framework.Infrastructure.Redis;
using TerraMours.Framework.Infrastructure.Services;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Res;
using TerraMours_Gpt.Domains.GptDomain.Hubs;
using TerraMours_Gpt.Domains.GptDomain.IServices;
using TerraMours_Gpt.Domains.GptDomain.Services;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Req;
using TerraMours_Gpt.Domains.LoginDomain.IServices;
using TerraMours_Gpt.Domains.LoginDomain.Services;
using TerraMours_Gpt.Domains.PayDomain.Contracts.Req;
using TerraMours_Gpt.Domains.PayDomain.Contracts.Res;
using TerraMours_Gpt.Domains.PayDomain.Hubs;
using TerraMours_Gpt.Domains.PayDomain.IServices;
using TerraMours_Gpt.Domains.PayDomain.Services;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.Commons;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.ProductModels;
using TerraMours_Gpt.Framework.Infrastructure.Middlewares;

//用于启用或禁用 Npgsql 客户端与 Postgres 服务器之间的时间戳行为。它并不会直接修改 Postgres 的时区设置。
//时间设置要提前，之前在身份检验后会导致失效
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

var builder = WebApplication.CreateBuilder(args);
//健康检查
builder.Services.AddHealthChecks()
//这里是添加自己的自定义的健康检查逻辑 使用默认的可以注释
    .AddCheck<HealthCheckService>("HealthCheck");
builder.Services.AddHealthChecksUI().AddInMemoryStorage();

//获取appsetting配置文件
IConfiguration configuration = builder.Configuration;

//添加配置文件与实体类绑定
builder.Services.Configure<SysSettings>(configuration.GetSection("SysSettings"));
var sysSettings = builder.Configuration.GetSection("SysSettings").Get<SysSettings>() ?? throw new Exception("用户或者密码不正确");
builder.Services.Configure<GptOptions>(configuration.GetSection("GptOptions"));
builder.Services.Configure<AlipayOptions>(configuration.GetSection("Alipay"));
//注入日志
// 配置 Serilog 日志记录器

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    //.WriteTo.File()
    .WriteTo.Seq(sysSettings.SeqUrl)
    .CreateLogger();
builder.Host.UseSerilog(Log.Logger);

//minimal Service 构造函数没有Ilog 会报错 
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
    //登录成功之后复制token,在swagger 右上角锁图标位置填入token
    //填入格式为Bearer xxxxxx   
    //注意Bearer后面有一个空格，后面再填入token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
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
    //配置XML备注文档
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TerraMours_Gpt.xml"));
});
//automapper
// 配置映射规则
//添加忽略null值的配置：.ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)
MapperConfiguration mapperConfig = new(cfg =>
{
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
});
//注册配置
IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

// 可用 启动自动验证 但是对外方法也要加东西 体验不好
builder.Services.AddFluentValidationAutoValidation();
//注入 ModifyUser2 ModifyUserIntendedEffect 对应上面两个cs文件
builder.Services.AddScoped<IValidator<SysUserReq>, SysUserReqValidator>();
builder.Services.AddScoped<IValidator<SysLoginUserReq>, SysLoginUserReqValidator>();


//添加EF Core数据库
// Add services to the container.
builder.Services.AddScoped<ISysUserService, SysUserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISysRoleService, SysRoleService>();
builder.Services.AddScoped<ISysMenuService, SysMenuService>();
builder.Services.AddScoped<ISeedDataService, SeedDataService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IAnalysisService, AnalysisService>();
//gpt
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IImageService, ImageService>();

//支付宝pay
builder.Services.AddScoped<IPayService, AliPayService>();

//商品服务
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyPolicy",
                      policy =>
                      {
                          policy.SetIsOriginAllowed(_ => true)
                                   .AllowAnyMethod()
                                   .AllowAnyHeader()
                          .AllowCredentials();
                      });
});
// Add Hangfire services.
builder.Services.AddHangfire(config => config.UseStorage(new PostgreSqlStorage(sysSettings.connection.DbConnectionString)));

//redis 缓存 这个实现了IDistributedCache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = sysSettings.connection.RedisHost;
    options.InstanceName = sysSettings.connection.RedisInstanceName;
});
builder.Services.AddScoped<IDistributedCacheHelper, DistributedCacheHelper>();


//过滤器
builder.Services.AddScoped<ExceptionFilter>();
//builder.Services.AddScoped<GlobalActionFilter>();

//添加认证  授权服务
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
    //从配置文件中获取key,这种方法需要新增一个类与之对应

    //var connStr = $"Host=localhost;Database=TerraMours;Username=postgres;Password=root";
    var connStr = sysSettings.connection.DbConnectionString;
    opt.UseNpgsql(connStr);
    //设置EF默认AsNoTracking,EF Core不 跟踪
    opt.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development)
    {
        //启用此选项后，EF Core将在日志中包含敏感数据，例如实体的属性值。这对于调试和排查问题非常有用。
        opt.EnableSensitiveDataLogging();
    }
});
builder.Services.AddScoped<FrameworkDbContext>();

//json小写的问题
builder.Services.Configure<JsonOptions>(options =>
{
    //net6的 options.JsonSerializerOptions.PropertyNamingPolicy = null;

    //net7 PropertyNameCaseInsensitive = true
    //保留原样字段名
    //options.SerializerOptions.PropertyNamingPolicy = null;
    //不区分大小写
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// SignalR
builder.Services.AddSignalR();

//添加限流中间件
/*var limiterName = "MyLimiterName";

var options = new RateLimiterOptions()
    .AddTokenBucketLimiter(limiterName, new TokenBucketRateLimiterOptions(1, QueueProcessingOrder.OldestFirst, 1, TimeSpan.FromSeconds(8), 1));

builder.Services.AddSingleton(options);
*/

// 添加Paylink依赖注入
builder.Services.AddAlipay();


//将builder.Build();注释掉然后 改为  builder.AddServices(); 自动注入我们写的服务（miniapi）即可，由于只是单体框架我们不需要使用caller，
//很简单的只是将miniapi代替以前的传统的controller而已,
//var app = builder.Build();
//添加masa miniapi
var app = builder.AddServices(opt =>
{
    opt.DisableAutoMapRoute = true;
});

//健康检查
//app.UseHealthChecks("/health");
app.UseHealthChecksUI();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

//日志
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// Use Hangfire server and dashboard.
app.UseHangfireServer(new BackgroundJobServerOptions
{
    Queues = new[] { "default", "img-queue" },
    WorkerCount = 1
});
app.UseHangfireDashboard();// 使用 Hangfire 控制面板

//app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory + "/images"),
    RequestPath = ""
});

//`UseCors` 添加 CORS 中间件。 对 `UseCors` 的调用必须放在 `UseRouting` 之后，但在 `UseAuthorization` 之前。 不然会出现前端获取不到response的现象
app.UseCors("MyPolicy");
//添加jwt验证
app.UseAuthentication();
app.UseAuthorization();


//请求中间件
app.UseMiddleware<KeyMiddleware>();

//使用minimal api
app.MapHealthChecks("/health", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

////可重写访问地址为http://localhost:5179/health-ui#/healthchecks
//app.MapHealthChecksUI(options => options.UIPath = "/health-ui");


//测试全局异常
//app.MapGet("/testError", () => { throw new Exception("测试异常"); }).AddEndpointFilter<ExceptionFilter>(); ;

//不使用minimal api
/*app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    //可重写访问地址为http://localhost:5179/health-ui#/healthchecks
    //endpoints.MapHealthChecksUI(options => options.UIPath = "/health-ui");
});
*/
// SignalR hub
app.MapHub<GraphGenerationHub>("/graphhub");
app.MapHub<PaymentHub>("/Hubs/QueryPaymentStatus");

app.Run();

