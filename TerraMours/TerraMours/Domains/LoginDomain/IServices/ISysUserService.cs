using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Domains.LoginDomain.Contracts.Req;

namespace TerraMours.Domains.LoginDomain.IServices
{
    public interface ISysUserService
    {
        Task<ApiResponse<string>> Register(SysUserReq userReq);
        Task<ApiResponse<LoginRes>> Login(SysLoginUserReq userReq);
        Task<ApiResponse<SysUserRes>> GetUserInfo(string userEmail);
        Task<ApiResponse<List<SysUserDetailRes>>> GetAllUserList();
        Task<ApiResponse<bool>> DelUser(SysUserBaseReq userReq);
        Task<ApiResponse<bool>> UpdateUser(SysUserDetailRes userReq);
    }
}
