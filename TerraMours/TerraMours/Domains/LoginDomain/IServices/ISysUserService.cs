using TerraMours.Domains.LoginDomain.Contracts.Req;

namespace TerraMours.Domains.LoginDomain.IServices
{
    public interface ISysUserService
    {
        Task<string> Login(SysLoginUserReq userReq);
        Task<string> Register(SysUserReq userReq);
    }
}
