namespace RenTN.Domain.Entities;

public class ApartmentPhoto
{
    public int ID { get; set; }
    public Apartment Apartment { get; set; } = default!;
    public int ApartmentID { get; set; } = default!;
    public string Url { get; set; } = default!;
    public bool IsDeleted { get; set; }

    public static ApartmentPhoto Clone(ApartmentPhoto apartmentPhoto)
    {
        return new ApartmentPhoto()
        {
            ID = apartmentPhoto.ID,
            ApartmentID = apartmentPhoto.ApartmentID,
            Url = apartmentPhoto.Url,
            IsDeleted = apartmentPhoto.IsDeleted,
        };
    }
}
