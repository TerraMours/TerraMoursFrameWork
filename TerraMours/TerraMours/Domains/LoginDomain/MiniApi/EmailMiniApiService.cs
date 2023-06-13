using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours.Domains.LoginDomain.IServices;

namespace TerraMours.Domains.LoginDomain.MiniApi
{
    public class EmailMiniApiService : ServiceBase
    {
        private readonly IEmailService _emailService;
        public EmailMiniApiService(IServiceCollection services, IEmailService emailService) : base()
        {
            _emailService = emailService;
            //此处/api/v1/Test 这里是swagger显示的路由
            //命名规则取当前的xxxMiniApiService的xxx,然后/api/v1/xxx/方法名
            App.MapPost("/api/v1/Email/CreateCheckCode", CreateCheckCode);
        }

        /// <summary>
        /// 邮件发验证码
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IResult> CreateCheckCode([FromBody] EmailReq req)
        {
            var res = await _emailService.CreateCheckCode(req);
            return Results.Ok(res);
        }
    }
}
