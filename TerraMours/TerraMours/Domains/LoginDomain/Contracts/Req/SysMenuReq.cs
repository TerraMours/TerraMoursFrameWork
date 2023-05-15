using System.Text.Json.Serialization;

namespace TerraMours.Domains.LoginDomain.Contracts.Req
{
    public class SysMenuReq
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public int UserId { get; set; }
    }
}
