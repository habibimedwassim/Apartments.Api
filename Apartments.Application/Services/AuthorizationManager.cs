using Apartments.Application.Common;
using Apartments.Application.IServices;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Services;

public class AuthorizationManager(ILogger<AuthorizationManager> logger) : IAuthorizationManager
{
    public bool AuthorizeUserReport(CurrentUser user, ResourceOperation operation, UserReport userReport)
    {
        if (user.IsAdmin) return true;

        if (operation == ResourceOperation.Delete && userReport.ReporterId == user.Id)
        {
            return true;
        }

        if (operation == ResourceOperation.Update)
        {
            if (user.IsUser && userReport.ReporterId == user.Id) return true;

            if(user.IsOwner && (userReport.ReporterId == user.Id || userReport.TargetId == user.Id))
            {
                return true;
            }
        }

        return false;
    }
    public bool AuthorizeApartment(CurrentUser user, ResourceOperation operation, Apartment? apartment = null)
    {
        if (user.IsAdmin) return true;

        if (user.IsUser) return false;

        if (operation == ResourceOperation.Create && user.IsOwner)
        {
            logger.LogInformation("Owner authorized to create apartment.");
            return true;
        }

        if (IsTheOwner(user, apartment))
            if (operation == ResourceOperation.Update ||
                operation == ResourceOperation.Delete ||
                operation == ResourceOperation.Restore)
            {
                logger.LogInformation("Owner authorized to {Operation} apartment.", operation.ToString());
                return true;
            }

        return LogAndDeny(user, operation, nameof(Apartment));
    }

    public bool AuthorizeApartmentPhoto(CurrentUser user, ResourceOperation operation, string? ownerId = null)
    {
        if (user.IsAdmin) return true;

        if (ownerId != null && user.IsOwner && ownerId == user.Id)
            if (operation == ResourceOperation.Create ||
                operation == ResourceOperation.Delete ||
                operation == ResourceOperation.Restore)
            {
                logger.LogInformation("Owner authorized to {Operation} apartment photos.", operation.ToString());
                return true;
            }

        return LogAndDeny(user, operation, nameof(ApartmentPhoto));
    }

    public bool AuthorizeRentTransaction(CurrentUser user, ResourceOperation operation,
        RentTransaction? rentTransaction = null)
    {
        if (user.IsAdmin) return true;

        if (user.IsUser && operation == ResourceOperation.Create)
        {
            logger.LogInformation("User authorized to create a rent transaction.");
            return true;
        }

        if (IsTheOwner(user, rentTransaction) &&
            (operation == ResourceOperation.Update || operation == ResourceOperation.Delete))
        {
            logger.LogInformation("Owner authorized to Update the rent transaction.");
            return true;
        }

        if (IsTheTenant(user, rentTransaction) && operation == ResourceOperation.Cancel)
        {
            logger.LogInformation("User authorized to Cancel the rent transaction.");
            return true;
        }

        return LogAndDeny(user, operation, nameof(RentTransaction));
    }

    public bool AuthorizeApartmentRequest(CurrentUser user, ResourceOperation operation, ApartmentRequestType type,
        ApartmentRequest? apartmentRequest = null)
    {
        if (user.IsAdmin) return true;

        return type switch
        {
            ApartmentRequestType.Rent => AuthorizeRentRequest(user, operation, apartmentRequest),
            ApartmentRequestType.Dismiss => AuthorizeDismissRequest(user, operation, apartmentRequest),
            ApartmentRequestType.Leave => AuthorizeLeaveRequest(user, operation, apartmentRequest),
            _ => LogAndDeny(user, operation, nameof(ApartmentRequest))
        };
    }

    private bool AuthorizeRentRequest(CurrentUser user, ResourceOperation operation, ApartmentRequest? apartmentRequest)
    {
        if (operation == ResourceOperation.Create && user.IsUser)
        {
            logger.LogInformation("User authorized to apply for an apartment.");
            return true;
        }

        if (IsTheOwner(user, apartmentRequest))
            if (operation == ResourceOperation.Update ||
                operation == ResourceOperation.Delete ||
                operation == ResourceOperation.ApproveReject)
            {
                logger.LogInformation("Owner authorized to {Operation} the rent request.", operation.ToString());
                return true;
            }

        if (IsTheTenant(user, apartmentRequest) && operation == ResourceOperation.Cancel)
        {
            logger.LogInformation("Tenant authorized to Cancel the rent request.");
            return true;
        }

        return LogAndDeny(user, operation, nameof(ApartmentRequest));
    }

    private bool AuthorizeDismissRequest(CurrentUser user, ResourceOperation operation,
        ApartmentRequest? apartmentRequest)
    {
        if (IsTheOwner(user, apartmentRequest?.Apartment) && operation == ResourceOperation.Create)
        {
            logger.LogInformation("Owner authorized to dismiss the tenant.");
            return true;
        }

        if (IsTheOwner(user, apartmentRequest))
            if (operation == ResourceOperation.Update || operation == ResourceOperation.Delete)
            {
                logger.LogInformation("Owner authorized to {Operation} the dismiss request.", operation.ToString());
                return true;
            }

        return LogAndDeny(user, operation, nameof(ApartmentRequest));
    }

    private bool AuthorizeLeaveRequest(CurrentUser user, ResourceOperation operation,
        ApartmentRequest? apartmentRequest)
    {
        if (user.IsUser && operation == ResourceOperation.Create)
        {
            logger.LogInformation("User authorized to Create a leave request.");
            return true;
        }

        if (IsTheTenant(user, apartmentRequest) && (operation == ResourceOperation.Cancel || operation == ResourceOperation.Update))
        {
            logger.LogInformation("User authorized to Cancel the leave request.");
            return true;
        }

        if (IsTheOwner(user, apartmentRequest) && operation == ResourceOperation.ApproveReject)
        {
            logger.LogInformation("Owner authorized to Approve/Reject the leave request.");
            return true;
        }

        return LogAndDeny(user, operation, nameof(ApartmentRequest));
    }

    private bool IsTheOwner(CurrentUser user, object? requestObject)
    {
        return CheckUserRole(user, requestObject, "OwnerId", user.IsOwner);
    }

    private bool IsTheTenant(CurrentUser user, object? requestObject)
    {
        return CheckUserRole(user, requestObject, "TenantId", user.IsUser);
    }

    private bool CheckUserRole(CurrentUser user, object? requestObject, string propertyName, bool roleCondition)
    {
        if (requestObject == null || !roleCondition) return false;

        var userIdProperty = requestObject.GetType().GetProperty(propertyName);
        if (userIdProperty == null) return false;

        var userId = userIdProperty.GetValue(requestObject)?.ToString();
        return user.Id == userId;
    }

    private bool LogAndDeny(CurrentUser user, ResourceOperation operation, string resourceType)
    {
        logger.LogWarning("Authorization failed for user {UserEmail} to {Operation} a {ResourceType}.", user.Email,
            operation, resourceType);
        return false;
    }
}