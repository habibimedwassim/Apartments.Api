namespace RenTN.Application.DTOs.ApartmentDTOs;

public class ApartmentDTO
{
    public int ID { get; set; }
    public LocationDTO Location { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int Size { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public List<ApartmentPhotoDTO> ApartmentPhotos { get; set; } = [];
}
