namespace TerraMours.Framework.Infrastructure.Contracts.Commons
{
    public class SysSettings
    {
        public JWT jwt { get; set; }
        public Connection connection { get; set; }
        public Secret secret { get; set; }

        public Email email { get; set; }
    }


    public class Connection
    {
        public string DBType { get; set; }
        public string DbConnectionString { get; set; }
        public string RedisHost { get; set; }
        public string RedisInstanceName { get; set; }
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

    /// <summary>
    /// 加解密配置key
    /// </summary>
    public class Secret
    {
        /// <summary>
        /// 加密key
        /// </summary>
        public string Encrypt { get; set; }

    }

    /// <summary>
    /// 邮箱配置
    /// </summary>
    public class Email
    {
        /// <summary>
        /// 邮箱host qq是 smtp.qq.com
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// QQ邮箱端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 是否开启ssl协议
        /// </summary>
        public bool UseSsl { get; set; }
        /// <summary>
        /// 发送的邮箱账号
        /// </summary>
        public string SenderEmail { get; set; }
        /// <summary>
        /// 发送的邮箱名称类似于网名
        /// </summary>
        public string SenderName { get; set; }
        /// <summary>
        /// 邮箱的授权码而不是密码
        /// </summary>
        public string SenderPassword { get; set; }

    }
}
