namespace Apartments.Domain.Entities;

public class ApartmentPhoto
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public Apartment Apartment { get; set; } = default!;
    public int ApartmentId { get; set; } = default!;
    public string Url { get; set; } = default!;
    public bool IsDeleted { get; set; }
}