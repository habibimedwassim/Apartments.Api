using Apartments.Application.Common;
using Apartments.Application.Dtos.DashboardDtos;
using Apartments.Application.IServices;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Services;

public class DashboardService(
    ILogger<DashboardService> logger,
    IMapper mapper,
    IUserContext userContext,
    IDashboardRepository dashboardRepository
    
    ) : IDashboardService
{
    public async Task<ServiceResult<OwnerDashboardDto>> GetOwnerDashboard()
    {
        var currentUser = userContext.GetCurrentUser();
        logger.LogInformation("Retrieving owner '{Email}' dashboard info", currentUser.Email);

        if (!currentUser.IsOwner)
        {
            return ServiceResult<OwnerDashboardDto>.ErrorResult(StatusCodes.Status403Forbidden, "Unauthorized");
        }

        var dashboardDetails = await dashboardRepository.GetOwnerDashboardDetailsAsync(currentUser.Id);
        var dashboardDto = mapper.Map<OwnerDashboardDto>(dashboardDetails);

        return ServiceResult<OwnerDashboardDto>.SuccessResult(dashboardDto);
    }
}
