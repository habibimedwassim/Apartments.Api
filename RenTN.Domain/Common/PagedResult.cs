namespace RenTN.Domain.Common;

public class PagedResult<T>
{
    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber)
    {
        Items = items;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)(Constants.PageSize));
        ItemsFrom = Constants.PageSize * (pageNumber - 1) + 1;
        ItemsTo = Math.Min(totalCount, ItemsFrom + Constants.PageSize - 1);
    }
    public IEnumerable<T> Items { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int ItemsFrom { get; set; }
    public int ItemsTo { get; set; }
}