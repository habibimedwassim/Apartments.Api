using Apartments.Domain.Common;

namespace Apartments.Domain.QueryFilters;

public class ApartmentQueryFilter
{
    public int pageNumber { get; set; }
    public string? sortBy { get; set; }
    public SortDirection sortDirection { get; set; } = SortDirection.Descending;
    public string? city { get; set; }
    public string? street { get; set; }
    public string? postalCode { get; set; }
    public int? apartmentSize { get; set; }
    public decimal? minPrice { get; set; }
    public decimal? maxPrice { get; set; }
    public bool? isOccupied { get; set; }
    public DateOnly? availableFrom { get; set; }
}