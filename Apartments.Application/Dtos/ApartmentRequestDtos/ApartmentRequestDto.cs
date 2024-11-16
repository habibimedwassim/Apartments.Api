namespace Apartments.Application.Dtos.ApartmentRequestDtos;

public class ApartmentRequestDto
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public int ApartmentId { get; set; }
    public string Location { get; set; } = default!;
    public int TenantId { get; set; }
    public string? Avatar { get; set; }
    public DateOnly? RequestDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = default!;
    public string RequestType { get; set; } = default!;
    public bool IsDeleted { get; set; }
}