using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using TerraMours.Domains.LoginDomain.IServices;

namespace TerraMours.Domains.LoginDomain.MiniApi {
    public class UserMiniApiService :ServiceBase{
        private readonly ISysUserService _sysUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserMiniApiService(ISysUserService sysUserService, IHttpContextAccessor httpContextAccessor) : base() {
            _sysUserService = sysUserService;
            _httpContextAccessor = httpContextAccessor;
            App.MapGet("/api/v1/User/GetAllUserList", GetAllUserList);
        }
        /// <summary>
        /// 全部用户列表 todo：jwt添加权限
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetAllUserList() {
            var res=await _sysUserService.GetAllUserList();
            return Results.Ok(res);
        }
    }
}
