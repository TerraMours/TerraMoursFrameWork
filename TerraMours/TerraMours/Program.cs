using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TerraMours.Domains.LoginDomain.IServices;
using TerraMours.Domains.LoginDomain.Services;
using TerraMours.Framework.Infrastructure.Contracts.Commons;
using TerraMours.Framework.Infrastructure.EFCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//获取appsetting配置文件
IConfiguration configuration = builder.Configuration;

//添加配置文件与实体类绑定
builder.Services.Configure<SysSettings>(configuration.GetSection("SysSettings"));
var sysSettings = builder.Configuration.GetSection("SysSettings").Get<SysSettings>();
//添加EF Core数据库
// Add services to the container.
builder.Services.AddScoped<ISysUserService, SysUserService>();

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


app.UseHttpsRedirection();

//添加jwt验证
app.UseAuthentication();
app.UseAuthorization();

//用于启用或禁用 Npgsql 客户端与 Postgres 服务器之间的时间戳行为。它并不会直接修改 Postgres 的时区设置。
//AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.Run();

