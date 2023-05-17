using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours.Domains.LoginDomain.Contracts.Res;

namespace TerraMours.Domains.LoginDomain.IServices
{
    public interface ISysMenuService
    {
        Task<ApiResponse<List<SysMenuRes>>> GetAllMenuList();
        Task<ApiResponse<bool>> DelMenu(SysMenuBaseReq req);
        Task<ApiResponse<bool>> UpdateMenu(SysMenuReq req);
        Task<ApiResponse<bool>> AddMenu(SysMenuReq req);
    }
}
