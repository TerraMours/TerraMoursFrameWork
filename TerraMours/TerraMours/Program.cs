using Microsoft.EntityFrameworkCore;
using TerraMours.Framework.Infrastructure.EFCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//��ȡappsetting�����ļ�
//IConfiguration configuration = builder.Configuration;
//���EF Core���ݿ�
// Add services to the container.
builder.Services.AddDbContext<FrameworkDbContext>(opt =>
{
    //�������ļ��л�ȡkey,���ַ�����Ҫ����һ������֮��Ӧ

    //builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));
    //builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));
    //var connectionString = configuration.GetValue<string>("ConnStr");
    var connStr = $"Host=localhost;Database=TerraMours;Username=postgres;Password=root";
    opt.UseNpgsql(connStr);

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


app.UseHttpsRedirection();


app.Run();

