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

//获取appsetting配置文件
IConfiguration configuration = builder.Configuration;

//添加配置文件与实体类绑定
builder.Services.Configure<SysSettings>(configuration.GetSection("SysSettings"));
var sysSettings = builder.Configuration.GetSection("SysSettings").Get<SysSettings>();

//注入日志
// 配置 Serilog 日志记录器

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    //.WriteTo.File()
    .WriteTo.Seq("http://localhost:5341/")
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
});


// 可用 启动自动验证 但是对外方法也要加东西 体验不好
builder.Services.AddFluentValidationAutoValidation();
//注入 ModifyUser2 ModifyUserIntendedEffect 对应上面两个cs文件
builder.Services.AddScoped<IValidator<SysUserReq>, SysUserReqValidator>();

//添加EF Core数据库
// Add services to the container.
builder.Services.AddScoped<ISysUserService, SysUserService>();
builder.Services.AddScoped<IEmailService, EmailService>();




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

});

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



//将builder.Build();注释掉然后 改为  builder.AddServices(); 自动注入我们写的服务（miniapi）即可，由于只是单体框架我们不需要使用caller，
//很简单的只是将miniapi代替以前的传统的controller而已,
//var app = builder.Build();
//添加masa miniapi
var app = builder.AddServices();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//日志
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

//添加jwt验证
app.UseAuthentication();
app.UseAuthorization();

//用于启用或禁用 Npgsql 客户端与 Postgres 服务器之间的时间戳行为。它并不会直接修改 Postgres 的时区设置。
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

app.Run();

