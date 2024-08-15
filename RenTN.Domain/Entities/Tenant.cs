namespace RenTN.Domain.Entities;

public class Tenant
{
    public int ID { get; set; }
    public User User { get; set; } = default!;
    public string UserID { get; set; } = default!;
    public int ApartmentID { get; set; }
    public Apartment Apartment { get; set; } = default!;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public List<RentHistory> RentHistory { get; set; } = [];
}

