namespace TerraMours_Gpt.Domains.GptDomain.Contracts.Req {
    /// <summary>
    /// Completion参数类（用于高级配置）
    /// </summary>
    public class ChatCompletionBaseReq {
        /// <summary>
        /// 模型
        /// </summary>
        public string? Model { get; set; }
        public int? MaxTokens { get; set; }
        public float? TopP { get; set; }
        public int? N { get; set; }
        public string? Stop { get; set; }
        public float? PresencePenalty { get; set; }
        public float? FrequencyPenalty { get; set; }
        public float? Temperature { get; set; }
        public object? LogitBias { get; set; }
    }
}
