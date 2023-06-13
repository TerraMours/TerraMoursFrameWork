using Newtonsoft.Json.Linq;

namespace TerraMours.Framework.Infrastructure.Contracts.Commons {
    public class SysSettings {
        /// <summary>
        /// 新增用户默认角色ID
        /// </summary>
        public Initial initial { get; set; }
        public JWT jwt { get; set; }
        public Connection connection { get; set; }
        public Secret secret { get; set; }

        public Email email { get; set; }
    }


    public class Connection {
        public string DBType { get; set; }
        public string DbConnectionString { get; set; }
        public string RedisHost { get; set; }
        public string RedisInstanceName { get; set; }
    }

    public class JWT {
        /// <summary>
        /// Token 密钥
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
    public class Secret {
        /// <summary>
        /// 加密key
        /// </summary>
        public string Encrypt { get; set; }

    }

    /// <summary>
    /// 邮箱配置
    /// </summary>
    public class Email {
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
    /// <summary>
    /// 初始化参数
    /// </summary>
    public class Initial {
        /// <summary>
        /// 默认角色
        /// </summary>
        public long InitialRoleId { get; set; }
        /// <summary>
        /// 初始密码
        /// </summary>
        public string InitialPassWord { get; set; }

    }
    /// <summary>
    /// AI配置
    /// </summary>
    public class OpenAIOptions {
        public OpenAI OpenAI { get; set; }
        public AzureOpenAI AzureOpenAI { get; set; }
    }
    /// <summary>
    /// 图片生成配置
    /// </summary>
    public class ImagOptions {
        /// <summary>
        /// 生成图片的静态文件地址
        /// </summary>
        public string ImagFileBaseUrl { get; set; }

        public SDOptions SDOptions { get; set; }
    }
    /// <summary>
    /// Stable Diffusion 配置
    /// </summary>
    public class SDOptions {
        /// <summary>
        /// Stable Diffusion API地址
        /// </summary>
        public string BaseUrl { get; set; }
        /// <summary>
        /// 反向描述词
        /// </summary>
        public string Negative_Prompt { get; set; }
    }
    /// <summary>
    /// OpenAI配置
    /// </summary>
    public class OpenAI {
        /// <summary>
        /// OpenAI Key池
        /// </summary>
        public string[] KeyList { get; set; }
        public int MaxTokens { get; set; }
        public float Temperature { get; set; }
        public int FrequencyPenalty { get; set; }
        public int PresencePenalty { get; set; }
        public string ChatModel { get; set; }
        public string TextModel { get; set; }
        /// <summary>
        /// 上下文数量
        /// </summary>
        public int ContextCount { get; set; }
        /// <summary>
        /// 最大提问数量
        /// </summary>
        public int MaxQuestions { get; set; }
    }
    /// <summary>
    /// AzureOpenAI配置
    /// </summary>
    public class AzureOpenAI {
    }


}