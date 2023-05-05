namespace TerraMours.Framework.Infrastructure.Contracts.Commons
{
    public class SysSettings
    {
        public JWT jwt { get; set; }
        public Connection connection { get; set; }
    }


    public class Connection
    {
        public string DBType { get; set; }
        public string DbConnectionString { get; set; }
    }

    public class JWT
    {
        /// <summary>
        /// token 密钥
        /// </summary>
        public string SecretKey { get; set; }
        /// <summary>
        /// 发布者
        /// </summary>
        public string Issuer { get; set; }
        /// <summary>
        /// 接收者
        /// </summary>
        public string Audience { get; set; }
        /// <summary>
        /// 过期时间配置 默认配置天数 系统安全级别不高
        /// </summary>
        public string Expires { get; set; }

    }
}
