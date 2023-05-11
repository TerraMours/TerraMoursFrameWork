using System.Text.Json.Serialization;

namespace TerraMours.Domains.LoginDomain.Contracts.Req {
    public class SysUserDetailRes : SysUserRes{
        /// <summary>
        /// 邮箱 可以登录使用
        /// </summary>
        [JsonPropertyName("userEmail")]
        public string? UserEmail { get; set; }
        /// <summary>
        /// 用户手机号 国内需要验证11号位数 别的国家需要单独到时候加逻辑
        /// </summary>
        [JsonPropertyName("userPhoneNum")]
        public string? UserPhoneNum { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        [JsonPropertyName("gender")]
        public string? Gender { get; set; }
        /// <summary>
        /// 是否能登陆 账号被锁
        /// </summary>
        [JsonPropertyName("enableLogin")]
        public bool EnableLogin { get; set; }
    }
}
