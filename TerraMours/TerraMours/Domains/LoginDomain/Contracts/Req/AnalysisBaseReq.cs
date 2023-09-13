using TerraMours_Gpt.Domains.LoginDomain.Contracts.Enum;

namespace TerraMours_Gpt.Domains.LoginDomain.Contracts.Req
{
    /// <summary>
    /// 统计入参
    /// </summary>
    public class AnalysisBaseReq
    {
        /// <summary>
        /// 筛选类型
        /// </summary>
        public DateTypeEnum? DateType { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
