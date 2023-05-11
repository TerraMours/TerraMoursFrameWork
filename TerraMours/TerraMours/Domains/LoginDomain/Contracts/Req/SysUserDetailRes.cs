namespace TerraMours.Domains.LoginDomain.Contracts.Req {
    public class SysUserDetailRes : SysUserRes{
        /// <summary>
        /// 邮箱 可以登录使用
        /// </summary>
        public string? userEmail { get; set; }
        /// <summary>
        /// 用户手机号 国内需要验证11号位数 别的国家需要单独到时候加逻辑
        /// </summary>
        public string? userPhoneNum { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string? gender { get; set; }
        /// <summary>
        /// 是否能登陆 账号被锁
        /// </summary>
        public bool enableLogin { get; set; }
    }
}
