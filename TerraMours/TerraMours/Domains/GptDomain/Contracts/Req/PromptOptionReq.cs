namespace TerraMours_Gpt.Domains.GptDomain.Contracts.Req
{
    public class PromptOptionReq
    {
        /// <summary>
        /// 扮演
        /// </summary>
        public string? Act { get; set; }

        /// <summary>
        /// 提示词
        /// </summary>
        public string? Prompt { get; set; }
    }
}
