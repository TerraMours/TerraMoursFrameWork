namespace TerraMours_Gpt.Domains.LoginDomain.Contracts.Common
{
    public class PagedRes<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public PagedRes(IEnumerable<T> items, int totalItems, int pageIndex, int pageSize)
        {
            Items = items;
            TotalItems = totalItems;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }
    }

}
