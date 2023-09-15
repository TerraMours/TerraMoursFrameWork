using k8s.KubeConfigModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours.Domains.LoginDomain.Contracts.Res;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours.Framework.Infrastructure.Redis;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Enum;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Req;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Res;
using TerraMours_Gpt.Domains.LoginDomain.IServices;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.PaymentModels;

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
            List<TotalAnalysisRes> res =await GetAnalysisList(req);
            return ApiResponse<List<TotalAnalysisRes>>.Success(res);
        }

        

        public async Task<ApiResponse<List<AnalysisListRes>>> AllAnalysisList(AnalysisBaseReq req)
        {
            List<TotalAnalysisRes> chat = await GetAnalysisList(new AnalysisListReq( AnalysisTypeEnum.Ask, req));
            List<TotalAnalysisRes> image = await GetAnalysisList(new AnalysisListReq(AnalysisTypeEnum.Image, req));
            List<AnalysisListRes>res= MergeLists(chat, image);
            return ApiResponse<List<AnalysisListRes>>.Success(res);
        }

        public async Task<ApiResponse<List<TotalAnalysisRes>>> TotalAnalysis(AnalysisBaseReq req)
        {
            DateTime today = DateTime.Today;
            IQueryable<SysUser> userQuery = _dbContext.SysUsers;
            IQueryable<ChatRecord> chatQuery = _dbContext.ChatRecords;
            IQueryable<ImageRecord> imageQuery = _dbContext.ImageRecords;
            IQueryable<Order> orderQuery = _dbContext.Orders;
            List<TotalAnalysisRes> totalAnalysisRes = new List<TotalAnalysisRes>();
            var userTotal = await userQuery.CountAsync();
            var userLastTotaly = await userQuery.Where(u => u.CreateDate < today).CountAsync();
            totalAnalysisRes.Add(new TotalAnalysisRes() { Key = "用户总数", Total = userTotal,LastTotal = userLastTotaly });
            var useTotal = await chatQuery.Where(u => u.CreateDate >= today && u.CreateDate < today.AddDays(1)).Select(m=>m.UserId).Distinct().CountAsync();
            var useLastTotaly = await chatQuery.Where(u => u.CreateDate >= today.AddDays(-1) && u.CreateDate < today).Select(m => m.UserId).Distinct().CountAsync();
            totalAnalysisRes.Add(new TotalAnalysisRes() { Key = "上线用户数", Total = useTotal, LastTotal = useLastTotaly });
            var askTotal = await chatQuery.Where(u => u.CreateDate >= today && u.CreateDate < today.AddDays(1) && u.Role=="user").CountAsync();
            var askLastTotaly = await chatQuery.Where(u => u.CreateDate >= today.AddDays(-1) && u.CreateDate < today && u.Role == "user").CountAsync();
            totalAnalysisRes.Add(new TotalAnalysisRes() { Key = "提问量", Total = askTotal, LastTotal = askLastTotaly });
            var imageTotal = await imageQuery.Where(u => u.CreateDate >= today && u.CreateDate < today.AddDays(1)).CountAsync();
            var imageLastTotaly = await imageQuery.Where(u => u.CreateDate >= today.AddDays(-1) && u.CreateDate < today).CountAsync();
            totalAnalysisRes.Add(new TotalAnalysisRes() { Key = "图片生成量", Total = imageTotal, LastTotal = imageLastTotaly });
            var orderTotal = await orderQuery.Where(u =>  u.Status == "TRADE_SUCCESS").SumAsync(m=>m.Price);
            var orderLastTotaly = await orderQuery.Where(u =>  u.CreatedTime < today && u.Status == "TRADE_SUCCESS").SumAsync(m => m.Price);
            totalAnalysisRes.Add(new TotalAnalysisRes() { Key = "销售额", Total = (double)orderTotal, LastTotal = (double)orderLastTotaly });
            return ApiResponse<List<TotalAnalysisRes>>.Success(totalAnalysisRes);
        }

        #region 私有方法
        private async Task<List<TotalAnalysisRes>> GetAnalysisList(AnalysisListReq req) {
            List<TotalAnalysisRes> res = new List<TotalAnalysisRes>();
            switch (req.AnalysisType) {
                case AnalysisTypeEnum.Image:
                    IQueryable<ImageRecord> imageQuery = _dbContext.ImageRecords;
                    res = await GetAnalysis(req, imageQuery);
                    break;
                default:
                    res = await GetChat(req);
                    break;
            }

            return res;
        }
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
                    return await query.GroupBy(u => u.CreateDate.Value.Month).OrderBy(m => m.Key)
                        .Select(g => new TotalAnalysisRes { Key = g.Key.ToString(), Total = g.Count() })
                        .ToListAsync();
                default:
                    return await query.GroupBy(u => u.CreateDate.Value.Date).OrderBy(m => m.Key)
                        .Select(g => new TotalAnalysisRes { Key = g.Key.ToString("yyyy-MM-dd"), Total = g.Count() })
                        .ToListAsync();
            }
        }
        /// <summary>
        /// 聊天统计
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private async Task< List<TotalAnalysisRes>> GetChat(AnalysisBaseReq req) {
            IQueryable<ChatRecord> query = _dbContext.ChatRecords.Where(u=> u.Role == "user");
            switch (req.DateType) {
                case DateTypeEnum.Today:
                    DateTime today = DateTime.Today;
                    query = query.Where(u => u.CreateDate >= today && u.CreateDate < today.AddDays(1));
                    return await query.GroupBy(u => u.CreateDate.Hour).OrderBy(m => m.Key)
                        .Select(g => new TotalAnalysisRes { Key = $"{g.Key}:00", Total = g.Count() })
                        .ToListAsync();
                case DateTypeEnum.Month:
                    return await query.GroupBy(u => u.CreateDate.Month).OrderBy(m => m.Key)
                        .Select(g => new TotalAnalysisRes { Key = g.Key.ToString(), Total = g.Count() })
                        .ToListAsync();
                default:
                    return await query.GroupBy(u => u.CreateDate.Date).OrderBy(m => m.Key)
                        .Select(g => new TotalAnalysisRes { Key = g.Key.ToString("yyyy-MM-dd"), Total = g.Count() })
                        .ToListAsync();
            }
        }

        public List<AnalysisListRes> MergeLists(List<TotalAnalysisRes> listA, List<TotalAnalysisRes> listB) {
            Dictionary<string, AnalysisListRes> mergedDict = new Dictionary<string, AnalysisListRes>();

            void MergeItem(TotalAnalysisRes item, string property) {
                if (!mergedDict.TryGetValue(item.Key, out var analysisItem)) {
                    analysisItem = new AnalysisListRes { Key = item.Key };
                    mergedDict[item.Key] = analysisItem;
                }

                var total = (int)item.Total;

                switch (property) {
                    case "A": analysisItem.AskCount = total; break;
                    case "B": analysisItem.ImageCount = total; break;
                }
            }

            foreach (var item in listA) {
                MergeItem(item, "A");
            }

            foreach (var item in listB) {
                MergeItem(item, "B");
            }

            return mergedDict.Values.OrderBy(m=>m.Key).ToList();
        }

        #endregion
    }
}
