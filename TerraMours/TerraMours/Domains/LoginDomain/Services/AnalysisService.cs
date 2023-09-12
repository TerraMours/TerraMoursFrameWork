using Microsoft.EntityFrameworkCore;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Req;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Res;
using TerraMours_Gpt.Domains.LoginDomain.IServices;

namespace TerraMours_Gpt.Domains.LoginDomain.Services
{
    public class AnalysisService : IAnalysisService
    {
        private readonly FrameworkDbContext _dbContext;
        public AnalysisService(FrameworkDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<List<TotalAnalysisRes>>> TotalAnalysis(AnalysisBaseReq req)
        {
            List<TotalAnalysisRes> totalAnalysisRes = new List<TotalAnalysisRes>();
            var userTotal = await _dbContext.SysUsers.CountAsync();
            totalAnalysisRes.Add(new TotalAnalysisRes() { Key = "用户总数", Total = userTotal });
            return ApiResponse<List<TotalAnalysisRes>>.Success(totalAnalysisRes);
        }
    }
}
