using Apartments.Application.Dtos.UserDtos;

namespace Apartments.Application.Dtos.ApartmentDtos;

public class ApartmentPreviewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public decimal RentAmount { get; set; }
    public OwnerDto Owner { get; set; } = default!;
}
