using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentDtos;
using Apartments.Domain.Common;
using Apartments.Domain.QueryFilters;

namespace Apartments.Application.IServices;

public interface IApartmentService
{
    Task<PagedResult<ApartmentDto>> GetAllApartments(ApartmentQueryFilter apartmentQueryFilter);
    Task<ServiceResult<ApartmentDto>> GetApartmentById(int id);
    Task<ServiceResult<string>> CreateApartment(CreateApartmentDto createApartmentDTO);
    Task<ServiceResult<string>> UpdateApartment(int id, UpdateApartmentDto updateApartmentDto);
    Task<ServiceResult<string>> DeleteApartment(int id, bool permanent);
    Task<ServiceResult<IEnumerable<ApartmentDto>>> GetOwnedApartments(int? ownerId = null);
    Task<PagedResult<ApartmentDto>> GetOwnedApartmentsPaged(ApartmentQueryFilter apartmentQueryFilter);
    Task<PagedResult<ApartmentDto>> GetOwnedApartmentsPaged(int ownerId, int pageNumber);
}