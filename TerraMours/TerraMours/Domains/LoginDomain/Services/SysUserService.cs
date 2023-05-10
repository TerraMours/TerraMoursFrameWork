using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours.Domains.LoginDomain.IServices;
using TerraMours.Framework.Infrastructure.Contracts.Commons;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours.Framework.Infrastructure.Utils;

namespace TerraMours.Domains.LoginDomain.Services
{
    /// <summary>
    /// 登录api的实现类
    /// </summary>
    public class SysUserService : ISysUserService
    {
        private readonly FrameworkDbContext _dbContext;
        private readonly IOptionsSnapshot<SysSettings> _sysSettings;
        public SysUserService(FrameworkDbContext dbContext, IOptionsSnapshot<SysSettings> sysSettings)
        {
            _dbContext = dbContext;
            _sysSettings = sysSettings;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="userReq">用户登录请求信息</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> Login(SysUserReq userReq)
        {
            //todo 添加jwt，然后校验邮箱以及手机号等等，密码加密
            //登录
            try
            {
                //加密密码
                var encryptPwd = userReq.UserPassword.EncryptDES(_sysSettings.Value.secret.Encrypt);

                //查看数据库是否有此用户
                //目前只支持邮箱注册所以这里去判断UserEmail 即可，后续如果可以对接手机号注册 则加上手机号即可
                var user = await _dbContext.SysUsers.FirstOrDefaultAsync(x => x.UserEmail == userReq.UserAccount && x.UserPassword == encryptPwd) ?? throw new Exception("用户或者密码不正确");

                //需要使用到的Claims ,
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.UserEmail));
                //claims.Add(new Claim(ClaimTypes.NameIdentifier, user.));

                //生成token
                var token = CreateToken(claims);

                //更新数据库用户的的token
                user.Token = token;
                _dbContext.SysUsers.Update(user);
                _dbContext.SaveChanges();

                return token;
            }

            catch (Exception ex)
            {
                throw new Exception("" + ex.Message); ;
            }
        }

        /// <summary>
        /// 暂时系统只支持邮箱注册，todo： 后续会对接手机号注册登录
        /// </summary>
        /// <param name="userReq"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<string> Register(SysUserReq userReq)
        {
            try
            {
                //查看数据库是否有此用户
                //目前只支持邮箱注册所以这里去判断UserEmail 即可，后续如果可以对接手机号注册 则加上手机号即可
                //判断此邮箱是否已经被注册
                var user = await _dbContext.SysUsers.FirstOrDefaultAsync(x => x.UserEmail == userReq.UserAccount && x.UserPassword == userReq.UserPassword);
                if (user != null)
                {
                    return "用户已存在";
                }

                //判断邮件6位数 验证码是否正确
                //todo 编写mailService
                //根据用户的邮箱查询缓存里面的验证码是否正确或者过期
                var checkCode = CacheHelper.GetCache(userReq.UserAccount).ToString();

                if (userReq.CheckCode == checkCode)
                {
                    //加密密码
                    var encryptPwd = userReq.UserPassword.EncryptDES(_sysSettings.Value.secret.Encrypt);

                    var addUser = new SysUser(userReq.UserAccount, encryptPwd);
                    _dbContext.SysUsers.Add(addUser);

                    //更新数据库

                    var res = _dbContext.SaveChanges();

                    return "注册成功";
                }
                else
                {
                    return "验证码不正确或已过期";
                }

            }

            catch (Exception ex)
            {
                throw new Exception("" + ex.Message);
            }
        }

        /// <summary>
        /// 生成token
        /// </summary>
        /// <param name="claims">用户信息</param>
        /// <returns></returns>
        public string CreateToken(IEnumerable<Claim> claims)
        {
            // 1. 定义需要使用到的Claims ,由前端传过来

            // 2. 从 appsettings.json 中读取SecretKey
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_sysSettings.Value.jwt.SecretKey));

            // 3. 选择加密算法
            var algorithm = SecurityAlgorithms.HmacSha256;

            // 4. 生成Credentials
            var signingCredentials = new SigningCredentials(secretKey, algorithm);

            // 5. 从 appsettings.json 中读取Expires
            var expires = Convert.ToDouble(_sysSettings.Value.jwt.Expires);

            // 6. 根据以上，生成token
            var token = new JwtSecurityToken(
                _sysSettings.Value.jwt.Issuer,     //Issuer
                _sysSettings.Value.jwt.Audience,   //Audience
                claims,                          //Claims,
                DateTime.Now,                    //notBefore
                DateTime.Now.AddDays(expires),   //expires
                signingCredentials               //Credentials
            );

            // 7. 将token变为string
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }


    }
}
