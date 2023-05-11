using System.Text.Json.Serialization;

namespace TerraMours.Domains.LoginDomain.Contracts.Req
{
    public class SysUserReq
    {
        /// <summary>
        /// 登录账号 userEmail 或者 UserPhoneNum 因为可以使用邮箱或者手机号登录。
        /// </summary>
        public string userAccount { get; set; }

        /// <summary>
        /// 密码（是否加密）UserTrueName
        /// </summary>
        public string userPassword { get; set; }
        /// <summary>
        /// 注册时使用，保证两次密码一样
        /// </summary>
        public string repeatPassword { get; set; }
        /// <summary>
        /// 校验码
        /// </summary>
        public string checkCode { get; set; }
    }
}
