using Apartments.Application.Dtos.ApartmentDtos;
using Apartments.Application.Dtos.RentTransactionDtos;

namespace Apartments.Application.Dtos.UserDtos;

public class UserProfileDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public ApartmentDto? CurrentApartment { get; set; }
    public List<RentTransactionDto> RentTransactions { get; set; } = [];
}