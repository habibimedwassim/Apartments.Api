using Apartments.Domain.Common;

namespace Apartments.Domain.QueryFilters;

public class RentTransactionQueryFilter
{
    public int PageNumber { get; set; } = 1;
    public int? userId { get; set; }
    public string? SortBy { get; set; }
    public SortDirection SortDirection { get; set; } = SortDirection.Descending;
    public string? Status { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
}