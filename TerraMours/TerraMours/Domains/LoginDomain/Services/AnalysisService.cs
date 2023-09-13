using k8s.KubeConfigModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Domains.LoginDomain.Contracts.Res;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours.Framework.Infrastructure.Redis;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Enum;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Req;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Res;
using TerraMours_Gpt.Domains.LoginDomain.IServices;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels;

namespace TerraMours_Gpt.Domains.LoginDomain.Services
{
    public class AnalysisService : IAnalysisService
    {
        private readonly FrameworkDbContext _dbContext;
        private readonly IDistributedCacheHelper _helper;
        public AnalysisService(FrameworkDbContext dbContext, IDistributedCacheHelper helper)
        {
            _dbContext = dbContext;
            _helper = helper;
        }

        public async Task<ApiResponse<List<TotalAnalysisRes>>> AnalysisList(AnalysisListReq req)
        {
            List<TotalAnalysisRes> res = new List<TotalAnalysisRes>();
            switch (req.AnalysisType)
            {
                case AnalysisTypeEnum.Image:
                    res =await GetAnalysis(req, _dbContext.ImageRecords);
                    break;
                default:
                    res =await GetChat(req);
                    break;
            }
            return ApiResponse<List<TotalAnalysisRes>>.Success(res);
        }

        public Task<ApiResponse<List<AnalysisListRes>>> AllAnalysisList(AnalysisBaseReq req)
        {
            return null;
        }

        public async Task<ApiResponse<List<TotalAnalysisRes>>> TotalAnalysis(AnalysisBaseReq req)
        {
            DateTime today = DateTime.Today;
            List<TotalAnalysisRes> totalAnalysisRes = new List<TotalAnalysisRes>();
            var userTotal = await _dbContext.SysUsers.CountAsync();
            var userLastTotaly = await _dbContext.SysUsers.Where(u => u.CreateDate < today).CountAsync();
            totalAnalysisRes.Add(new TotalAnalysisRes() { Key = "用户总数", Total = userTotal,LastTotal = userLastTotaly });
            var useTotal = await _dbContext.ChatRecords.Where(u => u.CreateDate >= today && u.CreateDate < today.AddDays(1)).Select(m=>m.UserId).Distinct().CountAsync();
            var useLastTotaly = await _dbContext.ChatRecords.Where(u => u.CreateDate >= today.AddDays(-1) && u.CreateDate < today).Select(m => m.UserId).Distinct().CountAsync();
            totalAnalysisRes.Add(new TotalAnalysisRes() { Key = "上线用户数", Total = useTotal, LastTotal = useLastTotaly });
            var askTotal = await _dbContext.ChatRecords.Where(u => u.CreateDate >= today && u.CreateDate < today.AddDays(1) && u.Role=="user").CountAsync();
            var askLastTotaly = await _dbContext.ChatRecords.Where(u => u.CreateDate >= today.AddDays(-1) && u.CreateDate < today && u.Role == "user").CountAsync();
            totalAnalysisRes.Add(new TotalAnalysisRes() { Key = "提问量", Total = askTotal, LastTotal = askLastTotaly });
            var imageTotal = await _dbContext.ImageRecords.Where(u => u.CreateDate >= today && u.CreateDate < today.AddDays(1)).CountAsync();
            var imageLastTotaly = await _dbContext.ImageRecords.Where(u => u.CreateDate >= today.AddDays(-1) && u.CreateDate < today).CountAsync();
            totalAnalysisRes.Add(new TotalAnalysisRes() { Key = "图片生成量", Total = imageTotal, LastTotal = imageLastTotaly });
            var orderTotal = await _dbContext.Orders.Where(u =>  u.Status == "TRADE_SUCCESS").SumAsync(m=>m.Price);
            var orderLastTotaly = await _dbContext.Orders.Where(u =>  u.CreatedTime < today && u.Status == "TRADE_SUCCESS").SumAsync(m => m.Price);
            totalAnalysisRes.Add(new TotalAnalysisRes() { Key = "销售额", Total = (double)orderTotal, LastTotal = (double)orderLastTotaly });
            return ApiResponse<List<TotalAnalysisRes>>.Success(totalAnalysisRes);
        }

        #region 私有方法

        /// <summary>
        /// 统计( 泛型方法,适用于继承BaseEntity的实体)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="req"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task<List<TotalAnalysisRes>> GetAnalysis<T>(AnalysisBaseReq req, IQueryable<T> query) where T:BaseEntity
        {
            switch (req.DateType)
            {
                case DateTypeEnum.Today:
                    DateTime today = DateTime.Today;
                    query = query.Where(u => u.CreateDate >= today && u.CreateDate < today.AddDays(1));
                    return await query.GroupBy(u => u.CreateDate.Value.Hour).OrderBy(m => m.Key)
                        .Select(g => new TotalAnalysisRes { Key =$"{g.Key}:00" , Total = g.Count() })
                        .ToListAsync();
                case DateTypeEnum.Month:
                    if (req.StartTime != null && req.EndTime != null)
                        query = query.Where(u =>
                            u.CreateDate.Value.Month >= req.StartTime.Value.Month &&
                            u.CreateDate.Value.Month <= req.StartTime.Value.Month);
                    return await query.GroupBy(u => u.CreateDate.Value.Month).OrderBy(m => m.Key)
                        .Select(g => new TotalAnalysisRes { Key = g.Key.ToString(), Total = g.Count() })
                        .ToListAsync();
                default:
                    if (req.StartTime != null && req.EndTime != null)
                        query = query.Where(u =>
                        u.CreateDate.Value.Date >= req.StartTime.Value.Date &&
                        u.CreateDate.Value.Date <= req.StartTime.Value.Date);
                    return await query.GroupBy(u => u.CreateDate.Value.Date).OrderBy(m => m.Key)
                        .Select(g => new TotalAnalysisRes { Key = g.Key.ToString(), Total = g.Count() })
                        .ToListAsync();
            }
        }
        /// <summary>
        /// 聊天统计
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private async Task< List<TotalAnalysisRes>> GetChat(AnalysisBaseReq req) {
            IQueryable<ChatRecord> query = _dbContext.ChatRecords;
            switch (req.DateType) {
                case DateTypeEnum.Today:
                    DateTime today = DateTime.Today;
                    query = query.Where(u => u.CreateDate >= today && u.CreateDate < today.AddDays(1));
                    return await query.GroupBy(u => u.CreateDate.Hour).OrderBy(m => m.Key)
                        .Select(g => new TotalAnalysisRes { Key = $"{g.Key}:00", Total = g.Count() })
                        .ToListAsync();
                case DateTypeEnum.Month:
                    if (req.StartTime != null && req.EndTime != null)
                        query = query.Where(u =>
                            u.CreateDate.Month >= req.StartTime.Value.Month &&
                            u.CreateDate.Month <= req.StartTime.Value.Month);
                    return await query.GroupBy(u => u.CreateDate.Month).OrderBy(m => m.Key)
                        .Select(g => new TotalAnalysisRes { Key = g.Key.ToString(), Total = g.Count() })
                        .ToListAsync();
                default:
                    if (req.StartTime != null && req.EndTime != null)
                        query = query.Where(u =>
                        u.CreateDate.Date >= req.StartTime.Value.Date &&
                        u.CreateDate.Date <= req.StartTime.Value.Date);
                    return await query.GroupBy(u => u.CreateDate.Date).OrderBy(m => m.Key)
                        .Select(g => new TotalAnalysisRes { Key = g.Key.ToString(), Total = g.Count() })
                        .ToListAsync();
            }
        }
        #endregion
    }
}
