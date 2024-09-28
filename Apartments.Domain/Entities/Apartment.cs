namespace Apartments.Domain.Entities;

public class Apartment
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string OwnerId { get; set; } = default!;
    public User Owner { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int Size { get; set; }
    public decimal RentAmount { get; set; }
    public bool IsOccupied { get; set; } = false;
    public List<ApartmentPhoto> ApartmentPhotos { get; set; } = [];
    public bool IsDeleted { get; set; }
}
