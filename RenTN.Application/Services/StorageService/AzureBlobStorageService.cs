using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RenTN.Domain.Common;

namespace RenTN.Application.Services.StorageService;

public class AzureBlobStorageService(IOptions<AzureBlobStorageSettings> blobSettingsOptions): IAzureBlobStorageService
{
    private readonly AzureBlobStorageSettings _blobStorageSettings = blobSettingsOptions.Value;

    public async Task<string> UploadAsync(IFormFile file)
    {
        var _blobServiceClient = new BlobServiceClient(_blobStorageSettings.ConnectionString);
        var containerClient = _blobServiceClient.GetBlobContainerClient(_blobStorageSettings.ContainerName);

        // Get the file extension
        var fileExtension = Path.GetExtension(file.FileName);

        // Generate a unique name for the file with the extension
        var fileName = $"{Guid.NewGuid()}{fileExtension}";

        var blobClient = containerClient.GetBlobClient(fileName);
        
        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream);

        return blobClient.Uri.ToString();
    }
}
