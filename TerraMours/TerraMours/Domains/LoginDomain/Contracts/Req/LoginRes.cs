namespace TerraMours.Domains.LoginDomain.Contracts.Req
{
    public class LoginRes
    {
        public LoginRes(string token, string refreshToken)
        {
            this.token = token;
            this.refreshToken = refreshToken;
        }

        public string token { get; set; }
        public string refreshToken { get; set; }
    }

}
