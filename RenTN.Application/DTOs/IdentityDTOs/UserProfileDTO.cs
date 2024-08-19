using RenTN.Application.DTOs.ApartmentDTOs;

namespace RenTN.Application.DTOs.IdentityDTO;

public class UserProfileDTO
{
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public ApartmentDTO? CurrentApartment { get; set; }
}
