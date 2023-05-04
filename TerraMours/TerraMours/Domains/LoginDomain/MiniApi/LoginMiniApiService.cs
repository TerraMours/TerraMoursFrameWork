using Microsoft.AspNetCore.Mvc;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours.Domains.LoginDomain.IServices;

namespace TerraMours.Domains.LoginDomain.MiniApi
{
    public class LoginMiniApiService : ServiceBase
    {
        private readonly ISysUserService _sysUserService;

        public LoginMiniApiService(IServiceCollection services, ISysUserService sysUserService) : base()
        {
            _sysUserService = sysUserService;
            //此处/api/v1/Test 这里是swagger显示的路由
            //命名规则取当前的xxxMiniApiService的xxx,然后/api/v1/xxx/方法名
            App.MapPost("/api/v1/Login/Login", Login);
            App.MapGet("/api/v1/Login/Register", Register);
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <returns></returns>
        public async Task<IResult> Login([FromBody] SysUserReq userReq)
        {
            var res = await _sysUserService.Login(userReq);
            return Results.Ok(res);
        }

        /// <summary>
        /// 注册成功
        /// </summary>
        /// <returns></returns>
        public async Task<IResult> Register()
        {
            return Results.Ok("注册成功");
        }

    }
}
