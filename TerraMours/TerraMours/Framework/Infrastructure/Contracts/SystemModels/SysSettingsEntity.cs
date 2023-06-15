using Newtonsoft.Json.Linq;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;

namespace TerraMours_Gpt.Framework.Infrastructure.Contracts.SystemModels
{
    /// <summary>
    /// 系统设置
    /// </summary>
    public class SysSettingsEntity:BaseEntity
    {
        public long SysSettingsId { get; set; }
        /// <summary>
        /// 新建用户默认信息
        /// </summary>
        public JObject? Initial { get; set; }

        /// <summary>
        /// 邮箱配置
        /// </summary>
        public JObject? Email { get; set; }


    }
}
