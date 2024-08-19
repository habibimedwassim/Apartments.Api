namespace RenTN.Domain.Entities;

public class ApartmentPhoto
{
    public int ID { get; set; }
    public Apartment Apartment { get; set; } = default!;
    public int ApartmentID { get; set; } = default!;
    public string Url { get; set; } = default!;
    public bool IsDeleted { get; set; }

    public static ApartmentPhoto Clone(ApartmentPhoto apartment)
    {
        return new ApartmentPhoto
        {
            ID = apartment.ID,
            ApartmentID = apartment.ApartmentID,
            Url = apartment.Url,
            IsDeleted = apartment.IsDeleted,
        };
    }
}
