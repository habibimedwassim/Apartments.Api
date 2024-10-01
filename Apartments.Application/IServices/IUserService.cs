using Apartments.Application.Common;
using Apartments.Application.Dtos.UserDtos;

namespace Apartments.Application.IServices;

public interface IUserService
{
    Task<ServiceResult<UserDto>> GetUserProfile();
    Task<ServiceResult<string>> UpdateUserDetails(UpdateUserDto updateUserDto);
}