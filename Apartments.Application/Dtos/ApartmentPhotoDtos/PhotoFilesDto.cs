using Microsoft.AspNetCore.Http;

namespace Apartments.Application.Dtos.ApartmentPhotoDtos;

public class PhotoFilesDto
{
    public List<IFormFile> Photos { get; set; } = [];
}