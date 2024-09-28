using Apartments.Domain.Common;

namespace Apartments.Domain.Entities;

public class RentTransaction
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string TenantId { get; set; } = default!;
    public User Tenant { get; set; } = default!;
    public int ApartmentId { get; set; }
    public Apartment Apartment { get; set; } = default!;
    public string OwnerId { get; set; } = default!;
    public User Owner { get; set; } = default!;
    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public decimal RentAmount { get; set; }
    public string Status { get; set; } = RequestStatus.Pending;
    public bool IsDeleted { get; set; }
}