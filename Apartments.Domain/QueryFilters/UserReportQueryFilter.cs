using Apartments.Domain.Common;

namespace Apartments.Domain.QueryFilters;

public class UserReportQueryFilter
{
    public int PageNumber { get; set; } = 1;
    public string type { get; set; } = default!;
    public string? SortBy { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Descending;
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
