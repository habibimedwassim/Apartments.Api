using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Domain.Common;

namespace RenTN.Application.DTOs.RentalRequestDTOs;

public class RentalRequestDTO
{
    public int ID { get; set; }
    public RentalRequestTenantDTO Tenant { get; set; } = default!;
    public ApartmentDTO Apartment { get; set; } = default!;
    public DateOnly RequestDate { get; set; }
    public DateOnly? MeetingDate { get; set; }
    public RentalRequestStatus Status { get; set; }
}
