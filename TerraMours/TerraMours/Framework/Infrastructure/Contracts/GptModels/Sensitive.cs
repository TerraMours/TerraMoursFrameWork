using TerraMours.Framework.Infrastructure.Contracts.SystemModels;

namespace TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels
{
    /// <summary>
    /// 敏感词表
    /// </summary>
    public class Sensitive:BaseEntity
    {
        public long SensitiveId { get; set; }
        public string Word { get; set; }
    }
}
