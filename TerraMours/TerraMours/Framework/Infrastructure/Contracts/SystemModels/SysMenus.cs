namespace TerraMours.Framework.Infrastructure.Contracts.SystemModels
{
    /// <summary>
    /// 系统菜单表
    /// </summary>
    public class SysMenus : BaseEntity
    {
        /// <summary>
        /// 菜单id 主键 自增
        /// </summary>
        public long MenuId { get; set; }
        /// <summary>
        /// 菜单父级id 
        /// </summary>
        public long? ParentId { get; set; }
        /// <summary>
        /// 是否有子菜单 默认无
        /// </summary>
        public bool HasChildren { get; set; }
        /// <summary>
        /// 菜单名称 
        /// </summary>
        public string MenuName { get; set; }
        /// <summary>
        /// 菜单url地址 应该是sysurl + 菜单按钮 类似于相对路径的配置
        /// </summary>
        public string? MenuUrl { get; set; }
        /// <summary>
        /// 菜单图标  用不着 这个前端设置即可
        /// </summary>
        public string? Icon { get; set; }
        /// <summary>
        /// 菜单描述or说明
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string? IconUrl { get; set; }

        /// <summary>
        /// 菜单id 外键 与主键相等
        /// </summary>
        //public long MenuForeignId { get; set; }

        /// <summary>
        /// 每个菜单的按钮 外键就是 MenuId 自己
        /// </summary>
        public List<SysMenuButtons>? SysMenuButtons { get; set; }
    }
}
