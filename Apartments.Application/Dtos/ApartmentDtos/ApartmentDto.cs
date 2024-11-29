using Apartments.Application.Dtos.ApartmentPhotoDtos;

namespace Apartments.Application.Dtos.ApartmentDtos;

public class ApartmentDto
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Title { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int Size { get; set; }
    public decimal RentAmount { get; set; }
    public bool IsOccupied { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public List<ApartmentPhotoDto> ApartmentPhotos { get; set; } = [];
    public bool IsDeleted { get; set; }
    public string? OwnerEmail { get; set; }
    public string? OwnerPhone { get; set; }
    public int OwnerSysId { get; set; }
}