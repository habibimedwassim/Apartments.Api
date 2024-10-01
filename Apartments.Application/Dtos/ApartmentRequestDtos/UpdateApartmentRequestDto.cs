namespace Apartments.Application.Dtos.ApartmentRequestDtos;

public class UpdateApartmentRequestDto
{
    public DateOnly? RequestDate { get; set; }
    public string? Reason { get; set; }
    public string? Status { get; set; }
}