using System.Runtime.Serialization;
using YamlDotNet.Core.Tokens;

namespace TerraMours_Gpt.Domains.LoginDomain.Contracts.Enum
{
    /// <summary>
    /// 时间类型枚举
    /// </summary>
    public enum DateTypeEnum
    {
        [EnumMember(Value = "全部")]
        All,
        [EnumMember(Value = "今天")]
        today,
        [EnumMember(Value = "最近7天")]
        lately7,
        [EnumMember(Value = "最近30天")]
        lately30
    }
}
