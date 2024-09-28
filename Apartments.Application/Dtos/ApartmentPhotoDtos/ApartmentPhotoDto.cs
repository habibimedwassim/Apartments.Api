namespace Apartments.Application.Dtos.ApartmentPhotoDtos;

public class ApartmentPhotoDto
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Url { get; set; } = default!;
}