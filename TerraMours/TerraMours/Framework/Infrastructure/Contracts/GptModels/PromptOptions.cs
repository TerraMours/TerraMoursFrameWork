using TerraMours.Framework.Infrastructure.Contracts.SystemModels;

namespace TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels
{
    /// <summary>
    /// 提示词管理
    /// </summary>
    public class PromptOptions:BaseEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        public long PromptId { get; set; }

        /// <summary>
        /// 扮演
        /// </summary>
        public string? Act { get; set; }

        /// <summary>
        /// 提示词
        /// </summary>
        public string? Prompt { get; set; }

        /// <summary>
        /// 使用次数
        /// </summary>
        public int? UsedCount { get; set; }

    }
}
