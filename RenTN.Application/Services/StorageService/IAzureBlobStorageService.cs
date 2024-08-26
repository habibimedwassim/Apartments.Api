using Microsoft.AspNetCore.Http;

namespace RenTN.Application.Services.StorageService;

public interface IAzureBlobStorageService
{
    Task<string> UploadAsync(IFormFile file);
}
