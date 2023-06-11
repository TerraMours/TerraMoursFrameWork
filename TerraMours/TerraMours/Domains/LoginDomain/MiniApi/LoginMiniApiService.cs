using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours.Domains.LoginDomain.IServices;
using TerraMours.Framework.Infrastructure.Redis;

namespace TerraMours.Domains.LoginDomain.MiniApi
{
    public class LoginMiniApiService : ServiceBase
    {
        private readonly ISysUserService _sysUserService;
        private readonly Serilog.ILogger _log;
        private readonly IDistributedCacheHelper _helper;

        public LoginMiniApiService(IServiceCollection services, ISysUserService sysUserService, Serilog.ILogger log, IDistributedCacheHelper helper) : base()
        {
            _sysUserService = sysUserService;
            _log = log;
            _helper = helper;
            //此处/api/v1/Test 这里是swagger显示的路由
            //命名规则取当前的xxxMiniApiService的xxx,然后/api/v1/xxx/方法名
            App.MapPost("/api/v1/Login/Login", Login);
            App.MapPost("/api/v1/Login/Register", Register);
            App.MapPost("/api/v1/Login/Logout", Logout);
        }

        /// <summary>
        /// 用户登录   
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IResult> Login(IValidator<SysLoginUserReq> validator, SysLoginUserReq userReq)
        {
            var validationResult = await validator.ValidateAsync(userReq);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
            //测试seq
            //_log.Information("登录成功，测试Seq");

            //redis缓存测试
            //await _helper.GetOrCreateAsync("test", async e => "测试");

            var res = await _sysUserService.Login(userReq);
            return Results.Ok(res);
        }

        /// <summary>
        /// 注册成功，此方法注册的用户只是普通用户，不能访问后台管理页面，或者能登录，但是没有菜单显示即可
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IResult> Register(IValidator<SysUserReq> validator, [FromBody] SysUserReq userReq)
        {
            //这里可以用来验证返回，但是体验不好，后续优化 现在先放着，不做处理，注入方法貌似与controller不太一样，正常注入不生效
            //var result = await validator.ValidateAsync(userReq);
            var validationResult = await validator.ValidateAsync(userReq);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var res = await _sysUserService.Register(userReq);
            return Results.Ok(res);
        }

        /// <summary>
        /// 登出  //目前做法直接删除user的Token
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="userReq"></param>
        /// <returns></returns>
        public async Task<IResult> Logout(IValidator<SysLoginUserReq> validator, SysLoginUserReq userReq)
        {
            var validationResult = await validator.ValidateAsync(userReq);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }
            //测试seq
            //_log.Information("登录成功，测试Seq");

            //redis缓存测试
            //await _helper.GetOrCreateAsync("test", async e => "测试");

            var res = await _sysUserService.Logout(userReq);
            return Results.Ok(res);
        }

    }
}
