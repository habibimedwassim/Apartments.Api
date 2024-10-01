namespace Apartments.Domain.Entities;

public class ApartmentPhoto
{
    public int Id { get; init; }
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public Apartment Apartment { get; init; } = default!;
    public int ApartmentId { get; init; }
    public string Url { get; init; } = default!;
    public bool IsDeleted { get; set; }
}