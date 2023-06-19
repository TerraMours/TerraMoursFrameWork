using Microsoft.EntityFrameworkCore;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Framework.Infrastructure.Contracts.Commons;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours_Gpt.Domains.GptDomain.IServices;

namespace TerraMours_Gpt.Domains.GptDomain.Services
{
    /// <summary>
    /// 系统设置
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly FrameworkDbContext _dbContext;
        private readonly Serilog.ILogger _logger;

        public SettingsService(FrameworkDbContext dbContext, Serilog.ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> ChangeEmailSettings(Email email, long? userId)
        {
            var settings = await _dbContext.SysSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                return ApiResponse<bool>.Fail("未初始化数据");
            }
            settings.ChangeEmail(email,userId);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }

        public async Task<ApiResponse<Email>> GetEmailSettings()
        {
            var settings =await _dbContext.SysSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                return ApiResponse<Email>.Fail("未初始化数据");
            }
            return ApiResponse<Email>.Success(settings.Email);
        }
    }
}
