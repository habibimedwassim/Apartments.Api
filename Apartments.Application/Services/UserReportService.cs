using Apartments.Application.Common;
using Apartments.Application.Dtos.UserReportDtos;
using Apartments.Application.IServices;
using Apartments.Application.Utilities;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using Apartments.Domain.QueryFilters;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Services;

public class UserReportService(
    ILogger<UserReportService> logger,
    IMapper mapper,
    IUserContext userContext,
    IAuthorizationManager authorizationManager,
    INotificationDispatcher notificationDispatcher,
    INotificationRepository notificationRepository,
    IAzureBlobStorageService azureBlobStorageService,
    IUserReportRepository userReportRepository,
    IApartmentRepository apartmentRepository,
    IUserRepository userRepository,
    INotificationUtilities notificationUtilities
    ) : IUserReportService
{
    public async Task<ServiceResult<string>> CreateUserReport(CreateUserReportDto createUserReportDto)
    {
        var targetRole = CoreUtilities.ValidateEnum<ReportTarget>(createUserReportDto.TargetRole);
        createUserReportDto.TargetRole = targetRole.ToString();

        await using var transaction = await userRepository.BeginTransactionAsync();
        try
        {
            var currentUser = userContext.GetCurrentUser();
            logger.LogInformation("User: {UserEmail} creating new report for {TargetRole}", currentUser.Email, targetRole);

            string? targetId = null;
            if (createUserReportDto.ApartmentId.HasValue)
            {
                var apartment = await apartmentRepository.GetApartmentByIdAsync(createUserReportDto.ApartmentId.Value)
                                 ?? throw new NotFoundException("Apartment not found");
                if(apartment.TenantId != currentUser.Id)
                {
                    return ServiceResult<string>.ErrorResult(StatusCodes.Status403Forbidden, "You cannot report to this apartment owner");
                }
                targetId = apartment.OwnerId;
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

            await CreateAndSendNotifications(targetRole, targetId, "A New Report has been submitted for you");

            await transaction.CommitAsync();

            logger.LogInformation("Created user report by: {UserEmail}", currentUser.Email);
            return ServiceResult<string>.InfoResult(StatusCodes.Status201Created, "Report created successfully!");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Failed to create user report: {Message}", ex.Message);
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError, "Failed creating the Report!");
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
    public async Task<ServiceResult<PagedResult<UserReportDto>>> GetUserReportsPaged(UserReportQueryFilter filter)
    {
        var reportType = CoreUtilities.ValidateEnum<ReportType>(filter.type);

        logger.LogInformation("Retrieving Sent Reports");

        var currentUser = userContext.GetCurrentUser();

        var pagedModel = new PagedModel<UserReport>();

        if (reportType == ReportType.Sent)
        {
            pagedModel = await userReportRepository.GetSentReportsPagedAsync(filter, currentUser.Id);
        }
        else if(reportType == ReportType.Received)
        {
            var isAdmin = currentUser.Role == UserRoles.Admin;

            pagedModel = await userReportRepository.GetReceivedReportsPagedAsync(filter, currentUser.Id, isAdmin);
        }

        var userReportsDto = mapper.Map<IEnumerable<UserReportDto>>(pagedModel.Data);

        var result = new PagedResult<UserReportDto>(userReportsDto, pagedModel.DataCount, filter.PageNumber);

        return ServiceResult<PagedResult<UserReportDto>>.SuccessResult(result);
    }
    public async Task<ServiceResult<PagedResult<UserReportDto>>> GetSentReportsPaged(UserReportQueryFilter filter)
    {
        var reportType = CoreUtilities.ValidateEnum<ReportType>(filter.type);

        logger.LogInformation("Retrieving Sent Reports");

        var currentUser = userContext.GetCurrentUser();

        var pagedModel = new PagedModel<UserReport>();

        if(reportType == ReportType.Sent)
        {
            pagedModel = await userReportRepository.GetSentReportsPagedAsync(filter, currentUser.Id);
        }
        else
        {
            var isAdmin = currentUser.Role == UserRoles.Admin;

            pagedModel = await userReportRepository.GetReceivedReportsPagedAsync(filter, currentUser.Id, isAdmin);
        }

        var userReportsDto = mapper.Map<IEnumerable<UserReportDto>>(pagedModel.Data);

        var result = new PagedResult<UserReportDto>(userReportsDto, pagedModel.DataCount, filter.PageNumber);

        return ServiceResult<PagedResult<UserReportDto>>.SuccessResult(result);
    }

    public async Task<ServiceResult<PagedResult<UserReportDto>>> GetReceivedReportsPaged(UserReportQueryFilter filter)
    {
        logger.LogInformation("Retrieving Received Reports");

        var currentUser = userContext.GetCurrentUser();

        var isAdmin = currentUser.Role == UserRoles.Admin;

        var pagedModel = await userReportRepository.GetReceivedReportsPagedAsync(filter, currentUser.Id, isAdmin);

        var userReportsDto = mapper.Map<IEnumerable<UserReportDto>>(pagedModel.Data);

        var result = new PagedResult<UserReportDto>(userReportsDto, pagedModel.DataCount, filter.PageNumber);

        return ServiceResult<PagedResult<UserReportDto>>.SuccessResult(result);
    }

    //public async Task<ServiceResult<List<UserReportDto>>> GetUserReports()
    //{
    //    logger.LogInformation("Retrieving User Reports");

    //    var currentUser = userContext.GetCurrentUser();

    //    var userReports = new List<UserReport>();
    //    if (currentUser.IsAdmin) 
    //    {
    //        userReports = await userReportRepository.GetAdminReports();
    //    }
    //    else
    //    {
    //        userReports = await userReportRepository.GetMyReports(currentUser.Id);
    //    }

    //    var userReportsDto = mapper.Map<List<UserReportDto>>(userReports);

    //    return ServiceResult<List<UserReportDto>>.SuccessResult(userReportsDto);
    //}

    public async Task<ServiceResult<string>> UpdateUserReport(int id, UpdateUserReportDto updateReportDto)
    {
        logger.LogInformation($"Updating User Report: {id}");

        var currentUser = userContext.GetCurrentUser();

        // Validate status
        if (!string.IsNullOrEmpty(updateReportDto.Status))
        {
            var statusEnum = CoreUtilities.ValidateEnum<ReportStatus>(updateReportDto.Status);
            updateReportDto.Status = statusEnum.ToString();
        }

        // Fetch and validate report
        var report = await userReportRepository.GetReportByIdAsync(id) ??
                         throw new NotFoundException("Report not found");

        if (!authorizationManager.AuthorizeUserReport(currentUser, ResourceOperation.Update, report))
        {
            throw new ForbiddenException();
        }

        bool statusChanged = updateReportDto.Status != report.Status && updateReportDto.Status != null;

        await using var transaction = await userRepository.BeginTransactionAsync();
        try
        {
            // Update report with new details
            await PerformReportUpdate(report, updateReportDto, currentUser.Email);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, $"Error updating report {id} by {currentUser.Email}: {ex.Message}");
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError,
                "Failed updating the Report!");
        }

        // Notify user about the status change
        if (statusChanged)
        {
            await NotifyUserOfStatusChange(report, updateReportDto.Status!);
        }

        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, $"Report updated successfully to {report.Status}!");
    }

    private async Task PerformReportUpdate(UserReport report, UpdateUserReportDto updateReportDto, string updatedBy)
    {
        var attachmentUrl = report.AttachmentUrl;

        if (updateReportDto.Attachment != null)
        {
            var uploadedAttachment = await azureBlobStorageService.UploadSingleFileAsync(updateReportDto.Attachment);
            if (!string.IsNullOrEmpty(uploadedAttachment))
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

        await userReportRepository.UpdateAsync(originalRecord, report, updatedBy);
    }

    private async Task NotifyUserOfStatusChange(UserReport report, string newStatus)
    {
        try
        {
            var message = newStatus switch
            {
                nameof(ReportStatus.InProgress) => "Your report is now in progress.",
                nameof(ReportStatus.Resolved) => "Your report has been resolved.",
                nameof(ReportStatus.Closed) => "Your report has been closed.",
                _ => "Your report status has been updated."
            };

            var notificationModel = new NotificationModel
            {
                UserId = report.ReporterId,
                Email = report.Reporter.Email!,
                Title = "Report Update",
                Message = message,
                NotificationType = NotificationType.Report.ToString().ToLower(),
                SendFirebase = report.Reporter.Role == UserRoles.User,
                SendEmail = newStatus == nameof(ReportStatus.Resolved)
            };

            await notificationUtilities.SendNotificationAsync(notificationModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to notify user {report.Reporter.Email} about report status change: {ex.Message}");
        }
    }

    public async Task<ServiceResult<UserReportDto>> GetReportById(int id)
    {
        logger.LogInformation("Retrieving Report with Id = {Id}",id);

        var report = await userReportRepository.GetReportByIdAsync(id) ??
                     throw new NotFoundException("Report not found");

        var reportDto = mapper.Map<UserReportDto>(report);

        return ServiceResult<UserReportDto>.SuccessResult(reportDto);
    }

    private async Task CreateAndSendNotifications(ReportTarget targetRole, string? targetId, string notificationMessage)
    {
        var notificationType = NotificationType.Report.ToString().ToLower();
        if (targetRole == ReportTarget.Admin)
        {
            var admins = await userRepository.GetAdmins();
            var notificationsList = admins.Select(admin => new Notification
            {
                UserId = admin,
                Message = notificationMessage,
                Type = notificationType,
                IsRead = false
            }).ToList();

            await notificationDispatcher.SendBulkNotificationsAsync(admins, notificationMessage, notificationType);
            await notificationRepository.AddNotificationListAsync(notificationsList);
        }
        else if (targetId != null)
        {
            await notificationDispatcher.SendNotificationAsync(targetId, notificationMessage, notificationType);
            await notificationRepository.AddNotificationAsync(new Notification
            {
                UserId = targetId,
                Message = notificationMessage,
                Type = notificationType,
                IsRead = false
            });
        }
    }
}
