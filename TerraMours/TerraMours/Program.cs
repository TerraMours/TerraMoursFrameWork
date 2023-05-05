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
//��ȡappsetting�����ļ�
IConfiguration configuration = builder.Configuration;

//��������ļ���ʵ�����
builder.Services.Configure<SysSettings>(configuration.GetSection("SysSettings"));
var sysSettings = builder.Configuration.GetSection("SysSettings").Get<SysSettings>();
//���EF Core���ݿ�
// Add services to the container.
builder.Services.AddScoped<ISysUserService, SysUserService>();

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

//���jwt��֤
app.UseAuthentication();
app.UseAuthorization();

//�������û���� Npgsql �ͻ����� Postgres ������֮���ʱ�����Ϊ����������ֱ���޸� Postgres ��ʱ�����á�
//AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.Run();

