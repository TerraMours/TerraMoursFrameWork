namespace TerraMours.Framework.Infrastructure.Contracts.SystemModels
{
    public class SysRole : BaseEntity
    {
        /// <summary>
        /// 主键 角色id 自增
        /// </summary>
        public long RoleId { get; set; }
        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; }
        /// <summary>
        /// 父级ID
        /// </summary>
        public long? ParentId { get; set; }
        /// <summary>
        /// 是否有子角色
        /// </summary>
        public bool HasChildren { get; set; }
        /// <summary>
        /// 部门ID 方便后续扩展
        /// </summary>
        public long? DeptId { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string? DeptName { get; set; }

        /// <summary>
        /// 一个角色对应的多个权限（菜单）外键是roleid
        /// </summary>
        public List<SysRolesToMenu>? RolesToMenus { get; set; }
    }
}
