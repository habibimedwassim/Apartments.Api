using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Domain.Common;

namespace RenTN.Application.DTOs.IdentityDTO;

public class OwnerProfileDTO
{
    public int ID { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public List<ApartmentDTO> OwnedApartments { get; set; } = [];
}
