using RenTN.Application.DTOs.IdentityDTO;

namespace RenTN.Application.Services.AdminService;

public interface IAdminService
{
    Task<object> GetUserByIdAsync(int id);
    Task<IEnumerable<object>> GetUsersAsync(string? userRole = null);
}
