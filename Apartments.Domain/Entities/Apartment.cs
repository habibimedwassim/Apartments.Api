namespace Apartments.Domain.Entities;

public class Apartment(string ownerId)
{
    public int Id { get; init; }
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public string? TenantId { get; set; }
    public User? Tenant { get; set; }
    public string OwnerId { get; init; } = ownerId;
    public User Owner { get; init; } = default!;
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int Size { get; set; }
    public decimal RentAmount { get; set; }
    public bool IsOccupied { get; set; }
    public DateOnly? AvailableFrom { get; set; }
    public List<ApartmentPhoto> ApartmentPhotos { get; set; } = [];
    public bool IsDeleted { get; set; }
}