using Apartments.Application.Common;
using Apartments.Application.Dtos.UserReportDtos;
using Apartments.Application.IServices;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Services;

public class UserReportService(
    ILogger<UserReportService> logger,
    IMapper mapper,
    IUserContext userContext,
    IAuthorizationManager authorizationManager,
    IAzureBlobStorageService azureBlobStorageService,
    IUserReportRepository userReportRepository,
    IUserRepository userRepository
    ) : IUserReportService
{
    public async Task<ServiceResult<string>> CreateUserReport(CreateUserReportDto createUserReportDto)
    {
        await using var transaction = await userRepository.BeginTransactionAsync();
        try
        {
            var currentUser = userContext.GetCurrentUser();

            logger.LogInformation("User: {UserEmail} creating new report", currentUser.Email);

            string? targetId = null;

            if (createUserReportDto.TargetId.HasValue)
            {
                var targetUser = await userRepository.GetBySysIdAsync(createUserReportDto.TargetId.Value) ??
                                 throw new NotFoundException("Target User not found");
                targetId = targetUser.Id;
            }

            var attachmentUrl = await azureBlobStorageService.UploadSingleFileAsync(createUserReportDto.Attachment);

            var userReport = new UserReport
            {
                ReporterId = currentUser.Id,
                TargetId = targetId,
                TargetRole = createUserReportDto.TargetRole,
                Message = createUserReportDto.Message,
                AttachmentUrl = attachmentUrl,
            };

            await userReportRepository.AddReportAsync(userReport);

            await transaction.CommitAsync();

            logger.LogInformation($"Created user report: {currentUser.Email}");
            return ServiceResult<string>.InfoResult(StatusCodes.Status201Created, "Report created successfully!");
        }
        catch (Exception ex) 
        {   
            await transaction.RollbackAsync();
            logger.LogError(ex, ex.Message);
            return ServiceResult<string>.InfoResult(StatusCodes.Status500InternalServerError,
                "Failed creating the Report!");
        }
    }

    public async Task<ServiceResult<string>> DeleteUserReport(int id)
    {
        await using var transaction = await userRepository.BeginTransactionAsync();
        try
        {
            logger.LogInformation($"Deleting User Report: {id}");

            var currentUser = userContext.GetCurrentUser();

            var report = await userReportRepository.GetReportByIdAsync(id) ??
                         throw new NotFoundException("Report not found");

            if (!authorizationManager.AuthorizeUserReport(currentUser, ResourceOperation.Delete, report))
            {
                throw new ForbiddenException();
            }

            if (await azureBlobStorageService.DeleteAsync(report.AttachmentUrl))
            {
                await userReportRepository.DeleteAsync(report);
                await transaction.CommitAsync();
                return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Report deleted successfully!");
            }

            await transaction.RollbackAsync();
            logger.LogError("Failed deleting the Report {id}!", id);

            return ServiceResult<string>.InfoResult(StatusCodes.Status500InternalServerError,
                "Failed updating the Report!");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, ex.Message);
            return ServiceResult<string>.InfoResult(StatusCodes.Status500InternalServerError,
                "Failed updating the Report!");
        }
    }

    public async Task<ServiceResult<List<UserReportDto>>> GetUserReports()
    {
        logger.LogInformation("Retrieving User Reports");

        var currentUser = userContext.GetCurrentUser();

        var userReports = new List<UserReport>();
        if (currentUser.IsAdmin) 
        {
            userReports = await userReportRepository.GetAdminReports();
        }
        else
        {
            userReports = await userReportRepository.GetMyReports(currentUser.Id);
        }

        var userReportsDto = mapper.Map<List<UserReportDto>>(userReports);

        return ServiceResult<List<UserReportDto>>.SuccessResult(userReportsDto);
    }

    public async Task<ServiceResult<string>> UpdateUserReport(int id, UpdateUserReportDto updateReportDto)
    {
        await using var transaction = await userRepository.BeginTransactionAsync();
        try
        {
            logger.LogInformation($"Updating User Report: {id}");

            var currentUser = userContext.GetCurrentUser();

            var report = await userReportRepository.GetReportByIdAsync(id) ??
                         throw new NotFoundException("Report not found");

            if(!authorizationManager.AuthorizeUserReport(currentUser, ResourceOperation.Update, report))
            {
                throw new ForbiddenException();
            }

            var attachmentUrl = report.AttachmentUrl;
            if(updateReportDto.Attachment != null)
            {
                var uploadedAttachment = await azureBlobStorageService.UploadSingleFileAsync(updateReportDto.Attachment);
                if (uploadedAttachment != null) 
                {
                    attachmentUrl = uploadedAttachment;
                }
            }

            var originalRecord = mapper.Map<UserReport>(report);
            mapper.Map(updateReportDto, report);
            report.AttachmentUrl = attachmentUrl;

            if (updateReportDto.Status == ReportStatus.Resolved.ToString())
            {
                report.ResolvedDate = updateReportDto.ResolvedDate ?? DateTime.UtcNow;
            }

            await userReportRepository.UpdateAsync(originalRecord, report, currentUser.Email);
            await transaction.CommitAsync();

            return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Report updated successfully!");

        }
        catch (Exception ex) 
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, ex.Message);
            return ServiceResult<string>.InfoResult(StatusCodes.Status500InternalServerError,
                "Failed updating the Report!");
        }
    }
}
