using RenTN.Application.DTOs.ApartmentPhotoDTOs;

namespace RenTN.Application.DTOs.ApartmentDTOs;

public class ApartmentDTO
{
    public int ID { get; set; }
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int Size { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public List<ApartmentPhotoDTO> ApartmentPhotos { get; set; } = [];
}