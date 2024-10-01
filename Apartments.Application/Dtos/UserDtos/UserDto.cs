using Apartments.Application.Dtos.ApartmentDtos;

namespace Apartments.Application.Dtos.UserDtos;

public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public ApartmentDto? CurrentApartment { get; set; }
}