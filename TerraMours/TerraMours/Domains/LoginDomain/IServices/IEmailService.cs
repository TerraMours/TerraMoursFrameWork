using TerraMours.Domains.LoginDomain.Contracts.Req;

namespace TerraMours.Domains.LoginDomain.IServices
{
    public interface IEmailService
    {
        Task<string> CreateCheckCode(EmailReq req);
    }
}
