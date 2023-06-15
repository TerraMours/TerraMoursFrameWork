using TerraMours.Framework.Infrastructure.Contracts.SystemModels;

namespace TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels
{
    /// <summary>
    ///  key配置和使用情况表
    /// </summary>
    public class KeyConfig:BaseEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        public long KeyId { get; set; }
        /// <summary>
        /// ApiKey
        /// </summary>
        public string? ApiKey { get; set; }
       
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? ExpirationTime { get; set; }
        /// <summary>
        /// 使用量
        /// </summary>
        public decimal? Used { get; set; }
        /// <summary>
        /// 余额
        /// </summary>
        public decimal? UnUsed { get; set; }

        /// <summary>
        /// 总量
        /// </summary>
        public decimal? Total { get; set; }
    }
}
