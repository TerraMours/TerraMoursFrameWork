using TerraMours.Framework.Infrastructure.Contracts.SystemModels;

namespace TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels
{
    /// <summary>
    /// 图片生成记录
    /// </summary>
    public class ImageRecord: BaseEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        public long ImageRecordId { get; set; }

        /// <summary>
        /// 模型类型
        /// </summary>
        public int? ModelType { get; set; }
        /// <summary>
        /// 聊天模型
        /// </summary>
        public string? Model { get; set; }

        public string? IP { get; set; }
        /// <summary>
        /// 提问词
        /// </summary>
        public string? Prompt { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        public long? UserId { get; set; }
        /// <summary>
        /// 是否公开到图片广场
        /// </summary>
        public bool? IsPublic { get; set;}
        /// <summary>
        /// 转发量
        /// </summary>
        public int? ForwardCount { get; set; }

        /// <summary>
        /// 收藏量
        /// </summary>
        public int? CollectCount { get;set; }
        /// <summary>
        /// 点赞量
        /// </summary>
        public int? LikeCount { get; set;}
    }
}
