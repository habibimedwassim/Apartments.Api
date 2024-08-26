using Microsoft.AspNetCore.Http;

namespace RenTN.Application.DTOs.ApartmentDTOs;

public class CreateApartmentDTO
{
    public string City { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int Size { get; set; }
    public decimal Price { get; set; }
    public List<IFormFile> ApartmentPhotos { get; set; } = new();
}
