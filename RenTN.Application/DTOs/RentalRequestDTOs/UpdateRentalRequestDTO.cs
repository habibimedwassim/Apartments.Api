using RenTN.Domain.Common;

namespace RenTN.Application.DTOs.RentalRequestDTOs;

public class UpdateRentalRequestDTO
{
    public int ID { get; set; }
    public RentalRequestStatus Status { get; set; }
    public DateOnly? MeetingDate { get; set; }
}
