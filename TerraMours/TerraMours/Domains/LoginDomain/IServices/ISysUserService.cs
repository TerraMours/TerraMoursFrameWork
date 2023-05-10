using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Domains.LoginDomain.Contracts.Req;

namespace TerraMours.Domains.LoginDomain.IServices
{
    public interface ISysUserService
    {
        Task<ApiResponse<string>> Register(SysUserReq userReq);
        Task<ApiResponse<LoginRes>> Login(SysUserReq userReq);
    }
}
