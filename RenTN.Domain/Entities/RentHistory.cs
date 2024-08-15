namespace RenTN.Domain.Entities;

public class RentHistory
{
    public int ID { get; set; }
    public Tenant Tenant { get; set; } = default!;
    public int TenantID { get; set; } = default!;
    public Apartment Apartment { get; set; } = default!;
    public int ApartmentID { get; set; } = default!;
    public decimal Amount { get; set; }
    public DateOnly PaymentDate { get; set; }
    public bool IsLate { get; set; }
}
