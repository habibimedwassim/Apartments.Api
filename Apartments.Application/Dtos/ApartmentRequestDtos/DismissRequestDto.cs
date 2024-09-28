namespace Apartments.Application.Dtos.ApartmentRequestDtos;

public class DismissRequestDto
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}
