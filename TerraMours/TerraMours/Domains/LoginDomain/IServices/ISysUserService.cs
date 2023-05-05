using TerraMours.Domains.LoginDomain.Contracts.Req;

namespace TerraMours.Domains.LoginDomain.IServices
{
    public interface ISysUserService
    {
        Task<string> Login(SysUserReq userReq);
    }
}
