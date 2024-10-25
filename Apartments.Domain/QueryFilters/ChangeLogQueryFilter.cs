using Apartments.Domain.Common;

namespace Apartments.Domain.QueryFilters;

public class ChangeLogQueryFilter
{
    public int PageNumber { get; set; } = 1;
    public string? SortBy { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Descending;
}