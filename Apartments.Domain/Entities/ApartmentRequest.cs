using Apartments.Domain.Common;

namespace Apartments.Domain.Entities;

public class ApartmentRequest
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string TenantId { get; set; } = default!;
    public User Tenant { get; set; } = default!;
    public int ApartmentId { get; set; }
    public Apartment Apartment { get; set; } = default!;
    public string OwnerId { get; set; } = default!;
    public User Owner { get; set; } = default!;
    public DateOnly? MeetingDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = RequestStatus.Pending;
    public string RequestType { get; set; } = default!;
    public bool IsDeleted { get; set; }
}