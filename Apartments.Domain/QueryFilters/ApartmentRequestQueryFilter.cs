using Apartments.Domain.Common;

namespace Apartments.Domain.QueryFilters;

public class ApartmentRequestQueryFilter
{
    public int pageNumber { get; set; }
    public string type { get; set; } = default!;
    public string? sortBy { get; set; }
    public SortDirection sortDirection { get; set; } = SortDirection.Descending;
    public int? apartmentId { get; set; }
    public string? status { get; set; }
}
