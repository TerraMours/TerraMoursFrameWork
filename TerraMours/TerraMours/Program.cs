using Microsoft.EntityFrameworkCore;
using TerraMours.Framework.Infrastructure.EFCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//获取appsetting配置文件
//IConfiguration configuration = builder.Configuration;
//添加EF Core数据库
// Add services to the container.
builder.Services.AddDbContext<FrameworkDbContext>(opt =>
{
    //从配置文件中获取key,这种方法需要新增一个类与之对应

    //builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));
    //builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));
    //var connectionString = configuration.GetValue<string>("ConnStr");
    var connStr = $"Host=localhost;Database=TerraMours;Username=postgres;Password=root";
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


app.Run();

