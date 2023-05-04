using TerraMours.Domains.LoginDomain.Contracts.Req;

namespace TerraMours.Domains.LoginDomain.IServices
{
    public interface ISysUserService
    {
        Task<SysUserReq> Login(SysUserReq userReq);
    }
}
