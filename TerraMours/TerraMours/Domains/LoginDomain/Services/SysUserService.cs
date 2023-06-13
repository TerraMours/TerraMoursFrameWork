using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours.Domains.LoginDomain.IServices;
using TerraMours.Framework.Infrastructure.Contracts.Commons;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours.Framework.Infrastructure.Redis;
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
        private readonly IMapper _mapper;
        private readonly IDistributedCacheHelper _helper;
        public SysUserService(FrameworkDbContext dbContext, IOptionsSnapshot<SysSettings> sysSettings, IMapper mapper, IDistributedCacheHelper helper) {
            _dbContext = dbContext;
            _sysSettings = sysSettings;
            _mapper = mapper;
            _helper = helper;
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="userReq">用户登录请求信息</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ApiResponse<LoginRes>> Login(SysLoginUserReq userReq)
        {
            //todo 添加jwt，然后校验邮箱以及手机号等等，密码加密
            //登录
            try
            {
                //加密密码
                var encryptPwd = userReq.UserPassword.EncryptDES(_sysSettings.Value.secret.Encrypt);

                //查看数据库是否有此用户
                //目前只支持邮箱注册所以这里去判断UserEmail 即可，后续如果可以对接手机号注册 则加上手机号即可
                var user = await _dbContext.SysUsers.FirstOrDefaultAsync(x => x.UserEmail == userReq.UserAccount && x.UserPassword == encryptPwd);
                if(user == null)
                {
                    return ApiResponse<LoginRes>.Fail("用户或者密码不正确");
                }
                //需要使用到的Claims ,
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.UserEmail));
                claims.Add(new Claim(ClaimTypes.Role, user.RoleId.ToString()));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));

                //生成token
                var token = CreateToken(claims);

                //更新数据库用户的的token
                user.Token = token;
                _dbContext.SysUsers.Update(user);
                _dbContext.SaveChanges();

                return ApiResponse<LoginRes>.Success(new LoginRes(token, token));
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
        public async Task<ApiResponse<string>> Register(SysUserReq userReq)
        {
            try
            {
                //查看数据库是否有此用户
                //目前只支持邮箱注册所以这里去判断UserEmail 即可，后续如果可以对接手机号注册 则加上手机号即可
                //判断此邮箱是否已经被注册
                var user = await _dbContext.SysUsers.FirstOrDefaultAsync(x => x.UserEmail == userReq.UserAccount && x.UserPassword == userReq.UserPassword);
                if (user != null)
                {
                    return ApiResponse<string>.Success("用户已存在");
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
                    //注册用户默认角色
                    addUser.RoleId = _sysSettings.Value.initial.InitialRoleId;
                    _dbContext.SysUsers.Add(addUser);

                    //更新数据库

                    var res = _dbContext.SaveChanges();

                    return ApiResponse<string>.Success("注册成功");
                }
                else
                {
                    return ApiResponse<string>.Success("验证码不正确");
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
        /// <summary>
        /// 获取用户信息（通过用户email）
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        public async Task<ApiResponse<SysUserRes>> GetUserInfo(string userEmail) {
            var user = await _dbContext.SysUsers.FirstOrDefaultAsync(x => x.UserEmail == userEmail);
            if (user == null) {
                return ApiResponse<SysUserRes>.Fail("用户不存在");
            }
            return ApiResponse<SysUserRes>.Success(new SysUserRes(user.UserId,user.UserName, user.RoleId));
        }
        /// <summary>
        /// 全部用户列表 todo：jwt添加权限
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<List<SysUserDetailRes>>> GetAllUserList() {
            var userList = await _helper.GetOrCreateAsync("GetAllUserList", async options => {return await _dbContext.SysUsers.Where(x => x.Enable == true).ToListAsync(); });
            var userDetailList = _mapper.Map<List<SysUserDetailRes>>(userList);
            return ApiResponse<List<SysUserDetailRes>>.Success(userDetailList);
        }
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="userReq"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<bool>> DelUser(SysUserBaseReq userReq)
        {
            var user=await _dbContext.SysUsers.FirstOrDefaultAsync(m => m.UserId == userReq.UserId);
            if (user == null)
            {
                return ApiResponse<bool>.Fail("用户不存在");
            }
            user?.Delete();
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="userReq"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<bool>> UpdateUser(SysUserDetailRes userReq)
        {
            if(await _dbContext.SysUsers.AnyAsync(m=>m.UserEmail==userReq.UserEmail && m.UserId != userReq.UserId))
            {
                return ApiResponse<bool>.Fail("邮箱已注册！");
            }
            var user =await _dbContext.SysUsers.FirstOrDefaultAsync(m => m.UserId == userReq.UserId);
            _mapper.Map(userReq, user);
            _dbContext.SysUsers.Update(user);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
        /// <summary>
        /// 新增用户（管理员）
        /// </summary>
        /// <param name="userReq"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<bool>> AddUser(SysUserAddReq userReq)
        {
            if (await _dbContext.SysUsers.AnyAsync(m => m.UserEmail == userReq.UserEmail))
            {
                return ApiResponse<bool>.Fail("邮箱已注册！");
            }
            //初始密码
            var encryptPwd = _sysSettings.Value.initial.InitialPassWord.EncryptDES(_sysSettings.Value.secret.Encrypt);
            var user = new SysUser(userReq.UserEmail, encryptPwd);
            _mapper.Map(userReq, user);
            await _dbContext.SysUsers.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }

        /// <summary>
        /// 登出 
        /// </summary>
        /// <param name="userReq"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> Logout(SysLoginUserReq userReq)
        {

            //登录
            try
            {
                //加密密码
                var encryptPwd = userReq.UserPassword.EncryptDES(_sysSettings.Value.secret.Encrypt);

                //查看数据库是否有此用户
                //目前只支持邮箱注册所以这里去判断UserEmail 即可，后续如果可以对接手机号注册 则加上手机号即可
                var user = await _dbContext.SysUsers.FirstOrDefaultAsync(x => x.UserEmail == userReq.UserAccount && x.UserPassword == encryptPwd) ?? throw new Exception("用户或者密码不正确");
                //更新数据库用户的的token
                user.Logout();
                //更新用户信息
                _dbContext.SysUsers.Update(user);
                _dbContext.SaveChanges();

                return "登出成功";
            }
            catch (Exception ex)
            {
                throw new Exception("" + ex.Message); ;
            }
        }


    }
}
