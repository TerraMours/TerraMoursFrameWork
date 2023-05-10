using System.Text.Json.Serialization;

namespace TerraMours.Domains.LoginDomain.Contracts.Req
{
    public class EmailReq
    {
        [JsonPropertyName("email")]
        public string UserEmail { get; set; }
    }
}
