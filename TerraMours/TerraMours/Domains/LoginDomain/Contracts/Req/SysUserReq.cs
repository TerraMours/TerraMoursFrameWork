using System.Text.Json.Serialization;

namespace TerraMours.Domains.LoginDomain.Contracts.Req
{
    public class SysUserReq
    {
        /// <summary>
        /// 登录账号 UserEmail 或者 UserPhoneNum 因为可以使用邮箱或者手机号登录。
        /// </summary>
        [JsonPropertyName("userAccount")]
        public string UserAccount { get; set; }

        /// <summary>
        /// 密码（是否加密）UserTrueName
        /// </summary>
        [JsonPropertyName("userPassword")]
        public string UserPassword { get; set; }
        /// <summary>
        /// 注册时使用，保证两次密码一样
        /// </summary>
        [JsonPropertyName("repeatPassword")]
        public string RepeatPassword { get; set; }
        /// <summary>
        /// 校验码
        /// </summary>
        [JsonPropertyName("checkCode")]
        public string CheckCode { get; set; }
    }
}
