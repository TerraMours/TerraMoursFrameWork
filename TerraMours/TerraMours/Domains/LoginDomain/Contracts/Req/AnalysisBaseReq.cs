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
        public string? dateType;
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? startTime;
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? endTime;
    }
}
