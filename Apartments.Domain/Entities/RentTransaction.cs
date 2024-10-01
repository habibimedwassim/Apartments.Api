using Apartments.Domain.Common;

namespace Apartments.Domain.Entities;

public class RentTransaction
{
    public int Id { get; init; }
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    public string TenantId { get; init; } = default!;
    public User Tenant { get; init; } = default!;
    public int ApartmentId { get; init; }
    public Apartment Apartment { get; init; } = default!;
    public string OwnerId { get; init; } = default!;
    public User Owner { get; init; } = default!;
    public DateOnly DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public decimal RentAmount { get; set; }
    public string Status { get; set; } = RequestStatus.Pending;
    public bool IsDeleted { get; set; }
}