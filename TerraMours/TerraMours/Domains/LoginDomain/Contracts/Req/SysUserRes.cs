namespace TerraMours.Domains.LoginDomain.Contracts.Req {
    /// <summary>
    /// 用户信息
    /// </summary>
    public class SysUserRes {
        public SysUserRes(long userId, string userName, string roleId) {
            this.userId = userId;
            this.userName = userName;
            this.roleId = roleId;
        }

        public SysUserRes() {
        }

        /// <summary>
        /// 用户id
        /// </summary>
        public long userId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string userName { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public string roleId { get; set; }
    }
}
