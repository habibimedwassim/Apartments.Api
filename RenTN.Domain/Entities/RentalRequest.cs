using RenTN.Domain.Common;

namespace RenTN.Domain.Entities;

public class RentalRequest
{
    public int ID { get; set; }
    public string TenantID { get; set; } = default!;
    public User Tenant { get; set; } = default!;
    public int ApartmentID { get; set; }
    public Apartment Apartment { get; set; } = default!;
    public string OwnerID { get; set; } = default!;
    public User Owner { get; set; } = default!;
    public DateOnly RequestDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public RentalRequestStatus Status { get; set; } = RentalRequestStatus.Pending;
    public bool IsDeleted { get; set; }
}