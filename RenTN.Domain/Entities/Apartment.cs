namespace RenTN.Domain.Entities;

public class Apartment
{
    public int ID { get; set; }
    public string OwnerID { get; set; } = default!;
    public User Owner { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int Size { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public List<ApartmentPhoto> ApartmentPhotos { get; set; } = [];
    public bool IsDeleted { get; set; }

    public static Apartment Clone(Apartment apartment)
    {
        return new Apartment
        {
            ID = apartment.ID,
            OwnerID = apartment.OwnerID,
            Street = apartment.Street,
            City = apartment.City,
            PostalCode = apartment.PostalCode,
            Description = apartment.Description,
            Size = apartment.Size,
            Price = apartment.Price,
            IsAvailable = apartment.IsAvailable,
            IsDeleted = apartment.IsDeleted,
        };
    }
}