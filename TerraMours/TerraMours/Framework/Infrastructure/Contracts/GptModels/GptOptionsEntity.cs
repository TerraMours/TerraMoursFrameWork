using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;

namespace TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels
{
    /// <summary>
    /// Gpt配置表
    /// </summary>
    public class GptOptionsEntity:BaseEntity
    {
        [Key]
        public long GptOptionsId { get; set; }
        /// <summary>
        /// AI配置
        /// </summary>
        public JObject? OpenAIOptions { get; set; }
        /// <summary>
        /// 图片生成配置
        /// </summary>
        public JObject? ImagOptions { get; set; }
    }
}
