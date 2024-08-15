namespace RenTN.Domain.Entities;

public class Apartment
{
    public int ID { get; set; }
    public User Owner { get; set; } = default!;
    public string OwnerID { get; set; } = default!;
    public Location? Location { get; set; }
    public string Description { get; set; } = default!;
    public int Size { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public List<ApartmentPhoto> ApartmentPhotos { get; set; } = [];
}
