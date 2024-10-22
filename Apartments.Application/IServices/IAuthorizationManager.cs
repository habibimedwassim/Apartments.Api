using Apartments.Application.Common;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;

namespace Apartments.Application.IServices;

public interface IAuthorizationManager
{
    bool AuthorizeApartment(CurrentUser user, ResourceOperation operation, Apartment? apartment = null);
    bool AuthorizeApartmentPhoto(CurrentUser user, ResourceOperation operation, string? ownerId = null);

    bool AuthorizeRentTransaction(CurrentUser user, ResourceOperation operation,
        RentTransaction? rentTransaction = null);

    bool AuthorizeApartmentRequest(CurrentUser user, ResourceOperation operation, ApartmentRequestType type,
        ApartmentRequest? apartmentRequest = null);
    bool AuthorizeUserReport(CurrentUser user, ResourceOperation operation, UserReport userReport);
}