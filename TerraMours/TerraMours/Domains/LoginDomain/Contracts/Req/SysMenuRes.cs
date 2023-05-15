namespace TerraMours.Domains.LoginDomain.Contracts.Req
{
    /// <summary>
    /// 前端路由配置类
    /// </summary>
    public class SysMenuRes
    {
        /// <summary>
        /// 首页路由（第一次进系统时候展示页面路由）
        /// </summary>
        public string Home { get; set; }
        /// <summary>
        /// 路由集合
        /// </summary>
        public List<RouteObject> Routes { get; set; }
    }

    public class RouteObject
    {
        /// <summary>
        /// 路由名称(路由唯一标识) 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 路由路径
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 路由的组件
        /// </summary>
        public string Component { get; set; }
        /// <summary>
        /// 子路由
        /// </summary>
        public List<RouteObject>? Children { get; set; }
        /// <summary>
        /// 路由描述
        /// </summary>
        public Meta Meta { get; set; }
    }


    public class ChildRoute
    {
        /// <summary>
        /// 路由名称(路由唯一标识) 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 路由路径
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 路由的组件
        /// </summary>
        public string Component { get; set; }
        public Meta Meta { get; set; }
    }
    /// <summary>
    /// 路由描述
    /// </summary>
    public class Meta
    {
        /// <summary>
        /// 路由标题(可用来作document.title或者菜单的名称) 
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 需要登录权限
        /// </summary>
        public bool RequiresAuth { get; set; }
        /// <summary>
        /// 菜单和面包屑对应的图标
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 使用本地svg作为的菜单和面包屑对应的图标(assets/svg-icon文件夹的的svg文件名)
        /// </summary>
        public string LocalIcon { get; set; }
        /// <summary>
        /// 外链链接
        /// </summary>
        public string Href { get; set; }
        /// <summary>
        /// 是否在菜单中隐藏(一些列表、表格的详情页面需要通过参数跳转，所以不能显示在菜单中)
        /// </summary>
        public bool Hide { get; set; }
        /// <summary>
        /// 是否支持多个tab页签(默认一个，即相同name的路由会被替换)
        /// </summary>
        public bool MultiTab { get; set; }
        /// <summary>
        /// 当前路由需要选中的菜单项(用于跳转至不在左侧菜单显示的路由且需要高亮某个菜单的情况)
        /// </summary>
        public string ActiveMenu { get; set; }
        /// <summary>
        /// 作为单级路由的父级路由布局组件
        /// </summary>
        public string SingleLayout { get; set; }

        public string DynamicPath { get; set; }
        /// <summary>
        /// 路由顺序，可用于菜单的排序
        /// </summary>
        public int Order { get; set; }
    }
}
