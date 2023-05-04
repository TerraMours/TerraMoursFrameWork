using Microsoft.EntityFrameworkCore;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours.Domains.LoginDomain.IServices;
using TerraMours.Framework.Infrastructure.EFCore;

namespace TerraMours.Domains.LoginDomain.Services
{
    /// <summary>
    /// 登录api的实现类
    /// </summary>
    public class SysUserService : ISysUserService
    {
        private readonly FrameworkDbContext _dbContext;
        public SysUserService(FrameworkDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<SysUserReq> Login(SysUserReq userReq)
        {
            //todo 添加jwt，然后校验邮箱以及手机号等等，密码加密
            //登录
            try
            {
                //查看数据库是否有此用户
                var user = await _dbContext.SysUsers.FirstOrDefaultAsync(x => x.UserName == userReq.UserAccount && x.UserPassword == userReq.UserPassword) ?? throw new Exception("用户或者密码不正确");
                SysUserReq res = new SysUserReq();
                res.UserAccount = user.UserEmail;
                res.UserPassword = user.UserPassword;
                return res;
            }

            catch (Exception ex)
            {
                throw new Exception("" + ex.Message); ;
            }


        }
    }
}
