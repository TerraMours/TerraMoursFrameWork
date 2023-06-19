using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Framework.Infrastructure.Contracts.Commons;

namespace TerraMours_Gpt.Domains.GptDomain.IServices
{
    public interface ISettingsService
    {
        /// <summary>
        /// 邮箱配置
        /// </summary>
        /// <returns></returns>
        public Task<ApiResponse<Email>> GetEmailSettings();

        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        public Task<ApiResponse<bool>> ChangeEmailSettings(Email email, long? userId);
    }
}
