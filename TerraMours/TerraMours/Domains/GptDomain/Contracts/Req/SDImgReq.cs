namespace TerraMours_Gpt.Domains.GptDomain.Contracts.Req {
    /// <summary>
    /// SD接口调用
    /// </summary>
    public class SDImgReq {
        public string prompt { get; set; }
        public string negative_prompt { get; set; }
        public int steps { get; set; }
    }
}
