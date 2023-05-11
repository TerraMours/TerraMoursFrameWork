using System.Text.Json.Serialization;

namespace TerraMours.Domains.LoginDomain.Contracts.Req
{
    public class EmailReq
    {
        [JsonPropertyName("userEmail")]
        public string UserEmail { get; set; }
    }
}
