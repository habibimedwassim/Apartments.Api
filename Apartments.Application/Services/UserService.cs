using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentDtos;
using Apartments.Application.Dtos.UserDtos;
using Apartments.Application.IServices;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Services;

public class UserService(
    ILogger<UserService> logger,
    IMapper mapper,
    IUserContext userContext,
    IUserRepository userRepository,
    UserManager<User> userManager,
    IApartmentRepository apartmentRepository)
    : IUserService
{
    public async Task<ServiceResult<UserDto>> GetUserProfile()
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Retrieving User {userId} profile", currentUser.Email);

        var user = await userRepository.GetByUserIdAsync(currentUser.Id) ??
                   throw new NotFoundException("User not found");

        var apartment = await apartmentRepository.GetApartmentByTenantId(currentUser.Id);

        var userDto = mapper.Map<UserDto>(user);

        if(apartment != null)
        {
            var apartmentDto = mapper.Map<ApartmentDto>(apartment);
            userDto.CurrentApartment = apartmentDto;
        }

        return ServiceResult<UserDto>.SuccessResult(userDto);
    }

    public async Task<ServiceResult<string>> UpdateUserDetails(UpdateUserDto updateAppUserDto)
    {
        var currentUser = userContext.GetCurrentUser();

        var user = await userManager.FindByIdAsync(currentUser.Id) ??
                   throw new NotFoundException("User not found");

        mapper.Map(updateAppUserDto, user);
        await userManager.UpdateAsync(user);

        logger.LogInformation("User details updated successfully for {UserId}.", user.Id);
        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "User updated successfully.");
    }
}