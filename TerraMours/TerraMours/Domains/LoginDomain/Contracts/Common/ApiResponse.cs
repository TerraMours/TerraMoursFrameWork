namespace TerraMours.Domains.LoginDomain.Contracts.Common
{
    public class ApiResponse<T>
    {
        public string StatusCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
