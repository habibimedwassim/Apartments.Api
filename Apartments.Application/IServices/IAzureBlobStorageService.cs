using Microsoft.AspNetCore.Http;
using Apartments.Domain.Entities;
using Apartments.Application.Dtos.ApartmentPhotoDtos;

namespace Apartments.Application.IServices;

public interface IAzureBlobStorageService
{
    Task<string> UploadAsync(IFormFile file);
    Task<bool> DeleteAsync(string blobUri);
    Task<string> UpdateAsync(string blobUri, IFormFile file);
    Task<IEnumerable<ApartmentPhoto>> UploadApartmentPhotos(UploadApartmentPhotoDto addApartmentPhotosDto);
    Task DeletePhotosInBatchesAsync(List<string> orphanedPhotos, int batchSize);
    Task<IEnumerable<string>> FindMissingPhotosInAzureAsync(HashSet<string> dbPhotoUrls, int batchSize);
}