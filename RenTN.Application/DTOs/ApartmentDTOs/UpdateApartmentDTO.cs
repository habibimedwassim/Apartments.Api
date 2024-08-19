namespace RenTN.Application.DTOs.ApartmentDTOs;

public class UpdateApartmentDTO
{
    public int ID { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
    public string? Description { get; set; }
    public int? Size { get; set; }
    public decimal? Price { get; set; }
    public bool? IsAvailable { get; set; }
}
