namespace Apartments.Application.Dtos.ApartmentRequestDtos;

public class ApartmentRequestDto
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public int ApartmentId { get; set; }
    public DateOnly? MeetingDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = default!;
    public string RequestType { get; set; } = default!;
}
