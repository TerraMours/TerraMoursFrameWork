namespace TerraMours_Gpt.Domains.LoginDomain.Contracts.Common
{
    public class PageReq
    {
        public string? QueryString { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
