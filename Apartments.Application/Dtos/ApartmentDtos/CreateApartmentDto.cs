using Microsoft.AspNetCore.Http;

namespace Apartments.Application.Dtos.ApartmentDtos;

public class CreateApartmentDto
{
    public string City { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int Size { get; set; }
    public decimal RentAmount { get; set; }
    public List<IFormFile> ApartmentPhotos { get; set; } = [];
}