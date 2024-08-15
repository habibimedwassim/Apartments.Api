namespace RenTN.Domain.Entities;

public class ApartmentPhoto
{
    public int ID { get; set; }
    public Apartment Apartment { get; set; } = default!;
    public int ApartmentID { get; set; } = default!;
    public string Url { get; set; } = default!;
}
