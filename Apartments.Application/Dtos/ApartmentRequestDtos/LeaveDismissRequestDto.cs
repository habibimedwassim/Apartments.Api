namespace Apartments.Application.Dtos.ApartmentRequestDtos;

public class LeaveDismissRequestDto
{
    public DateOnly? RequestDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}