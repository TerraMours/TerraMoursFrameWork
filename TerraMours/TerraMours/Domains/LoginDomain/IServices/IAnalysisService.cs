using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Req;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Res;

namespace TerraMours_Gpt.Domains.LoginDomain.IServices
{
    public interface IAnalysisService
    {
        /// <summary>
        /// 数量统计
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<ApiResponse<List<TotalAnalysisRes>>> TotalAnalysis(AnalysisBaseReq req);
    }
}
