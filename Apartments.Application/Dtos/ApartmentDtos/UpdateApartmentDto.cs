namespace Apartments.Application.Dtos.ApartmentDtos;

public class UpdateApartmentDto
{
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? Description { get; set; }
    public int? Size { get; set; }
    public decimal? RentAmount { get; set; }
    public bool? IsOccupied { get; set; }
}
