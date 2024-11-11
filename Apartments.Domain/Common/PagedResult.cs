using Apartments.Domain.Exceptions;

namespace Apartments.Domain.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public int ItemsFrom { get; set; }
    public int ItemsTo { get; set; }
    public int PageNumber { get; set; }

    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber)
    {
        Items = items;
        TotalCount = totalCount;

        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        var totalPages = (int)Math.Ceiling(totalCount / (double)AppConstants.PageSize);

        if (pageNumber > totalPages && totalPages > 0)
            throw new BadRequestException($"Page number {pageNumber} is beyond the limit of {totalPages}.");

        TotalPages = totalPages;

        if (totalCount == 0)
        {
            ItemsFrom = 0;
            ItemsTo = 0;
        }
        else
        {
            ItemsFrom = AppConstants.PageSize * (pageNumber - 1) + 1;
            ItemsTo = Math.Min(totalCount, ItemsFrom + AppConstants.PageSize - 1);
        }

        PageNumber = pageNumber;
    }
}

public class PagedModel<T>
{
    public IEnumerable<T> Data { get; set; } = [];
    public int DataCount { get; set; }
}