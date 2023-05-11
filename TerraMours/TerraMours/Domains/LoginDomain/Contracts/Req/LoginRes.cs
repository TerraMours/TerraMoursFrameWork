using System.Text.Json.Serialization;

namespace TerraMours.Domains.LoginDomain.Contracts.Req
{
    public class LoginRes
    {
        public LoginRes(string token, string refreshToken)
        {
            this.Token = token;
            this.RefreshToken = refreshToken;
        }
        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
    }

}
