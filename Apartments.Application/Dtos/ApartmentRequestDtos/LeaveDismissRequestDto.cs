namespace Apartments.Application.Dtos.ApartmentRequestDtos;

public class LeaveDismissRequestDto
{
    public DateOnly? RequestDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public string Reason { get; set; } = string.Empty;
}