using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Req;
using TerraMours_Gpt.Domains.LoginDomain.IServices;

namespace TerraMours_Gpt.Domains.LoginDomain.MiniApi
{
    public class AnalysisMiniApiService: ServiceBase
    {
        private readonly IAnalysisService _analysisService;

        public AnalysisMiniApiService(IServiceCollection services, IAnalysisService analysisService) : base()
        {
            _analysisService = analysisService;
            App.MapPost("/api/v1/Analysis/TotalAnalysis", TotalAnalysis);
            App.MapPost("/api/v1/Analysis/AnalysisList", AnalysisList);
            App.MapPost("/api/v1/Analysis/AllAnalysisList", AllAnalysisList);
        }

        /// <summary>
        /// 统计数量
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> TotalAnalysis([FromBody] AnalysisBaseReq req)
        {
            var res = await _analysisService.TotalAnalysis(req);
            return Results.Ok(res);
        }

        /// <summary>
        /// 单个统计数量（图表）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<IResult> AnalysisList([FromBody] AnalysisListReq req) {
            var res = await _analysisService.AnalysisList(req);
            return Results.Ok(res);
        }

        /// <summary>
        /// 所有统计数量（图表）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> AllAnalysisList([FromBody] AnalysisBaseReq req) {
            var res = await _analysisService.AllAnalysisList(req);
            return Results.Ok(res);
        }

    }
}
