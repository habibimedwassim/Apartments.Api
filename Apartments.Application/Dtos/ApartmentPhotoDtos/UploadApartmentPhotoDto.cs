using Microsoft.AspNetCore.Http;

namespace Apartments.Application.Dtos.ApartmentPhotoDtos;

public class UploadApartmentPhotoDto
{
    public int ApartmentId { get; set; }
    public List<IFormFile> ApartmentPhotos { get; set; } = [];
}