using TerraMours_Gpt.Domains.LoginDomain.Contracts.Enum;

namespace TerraMours_Gpt.Domains.LoginDomain.Contracts.Req {
    public class AnalysisListReq:AnalysisBaseReq {
        public AnalysisTypeEnum? AnalysisType { get; set; }
    }
}
