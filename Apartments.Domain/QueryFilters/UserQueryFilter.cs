namespace Apartments.Domain.QueryFilters;

public class UserQueryFilter
{
    public int PageNumber { get; set; } = default!;
    public string? Role { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}