using Apartments.Application.Common;
using Apartments.Application.Dtos.AuthDtos;
using Apartments.Application.Dtos.UserDtos;

namespace Apartments.Application.IServices;

public interface IUserService
{
    Task<ServiceResult<UserDto>> GetUserProfile();
    Task<ServiceResult<string>> UpdateUserDetails(UpdateUserDto updateUserDto);
    Task<ServiceResult<string>> UpdateUserEmail(EmailDto updateEmailDto);
    Task<ServiceResult<string>> UpdateUserPassword(ChangePasswordDto changePasswordDto);
    Task<ServiceResult<string>> VerifyEmailAsync(VerifyNewEmailDto verifyEmailDTO);
}