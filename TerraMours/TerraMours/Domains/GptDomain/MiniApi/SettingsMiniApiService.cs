using TerraMours_Gpt.Domains.GptDomain.IServices;

namespace TerraMours_Gpt.Domains.GptDomain.MiniApi
{
    public class SettingsMiniApiService : ServiceBase
    {
        private readonly ISeedDataService _seedDataService;

        public SettingsMiniApiService(ISeedDataService seedDataService):base()
        {
            _seedDataService = seedDataService;
            App.MapGet("/api/v1/Settings/EnsureSeedData", EnsureSeedData);
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <returns></returns>
        public async Task<IResult> EnsureSeedData()
        {
            var res= await _seedDataService.EnsureSeedData();
            return Results.Ok(res);
        }
    }
}
