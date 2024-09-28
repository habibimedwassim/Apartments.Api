using Apartments.Application.Dtos.ApartmentPhotoDtos;
using Apartments.Application.IServices;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apartments.Application.Services;

public class AzureBlobStorageService(
    ILogger<AzureBlobStorageService> logger,
    IOptions<AzureBlobStorageSettings> blobSettingsOptions
    ) : IAzureBlobStorageService
{
    private readonly AzureBlobStorageSettings _blobStorageSettings = blobSettingsOptions.Value;

    private readonly List<string> allowedImageMimeTypes = new()
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/bmp",
        "image/tiff",
        "image/webp"
    };

    private readonly List<string> allowedImageExtensions = new()
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".bmp",
        ".tiff",
        ".webp"
    };

    public async Task<string> UploadAsync(IFormFile file)
    {
        try
        {
            var _blobServiceClient = new BlobServiceClient(_blobStorageSettings.ConnectionString);
            var containerClient = _blobServiceClient.GetBlobContainerClient(_blobStorageSettings.ContainerName);

            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";

            var blobClient = containerClient.GetBlobClient(fileName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = file.ContentType
            };

            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, blobHttpHeaders);

            return blobClient.Uri.ToString();
        }
        catch (Exception ex) 
        {
            logger.LogError(ex, ex.Message);
            throw new AzureException("Failed to upload one or more photos.");
        }
    }

    public async Task<bool> DeleteAsync(string blobUri)
    {
        try
        {
            var blobServiceClient = new BlobServiceClient(_blobStorageSettings.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_blobStorageSettings.ContainerName);

            var blobName = GetBlobNameFromUri(blobUri);
            var blobClient = containerClient.GetBlobClient(blobName);

            var result = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete the photo.");
            throw new AzureException("Failed to delete the photo.");
        }
    }

    public async Task<string> UpdateAsync(string blobUri, IFormFile file)
    {
        // Delete the existing blob
        await DeleteAsync(blobUri);

        // Upload the new file
        return await UploadAsync(file);
    }
    public async Task<IEnumerable<ApartmentPhoto>> UploadApartmentPhotos(UploadApartmentPhotoDto uploadApartmentPhotoDto)
    {
        var apartmentPhotos = new List<ApartmentPhoto>();

        var photosToUpload = uploadApartmentPhotoDto.ApartmentPhotos
                                                    .Where(x => allowedImageMimeTypes.Contains(x.ContentType.ToLower()) &&
                                                                allowedImageExtensions.Contains(Path.GetExtension(x.FileName).ToLower()));

        foreach (var photo in photosToUpload)
        {
            var photoUrl = await UploadAsync(photo);

            if (!string.IsNullOrEmpty(photoUrl))
            {
                var apartmentPhoto = new ApartmentPhoto
                {
                    ApartmentId = uploadApartmentPhotoDto.ApartmentId,
                    Url = photoUrl
                };

                apartmentPhotos.Add(apartmentPhoto);
            }
            else
            {
                logger.LogWarning("Failed to upload photo: {FileName}", photo.FileName);
            }
        }

        var ignoredPhotos = uploadApartmentPhotoDto.ApartmentPhotos.Except(photosToUpload);
        if (ignoredPhotos.Any())
        {
            foreach (var ignoredPhoto in ignoredPhotos)
            {
                logger.LogWarning("Ignored file: {FileName}, MIME type: {ContentType}, because it's not an image.", ignoredPhoto.FileName, ignoredPhoto.ContentType);
            }
        }

        return apartmentPhotos;
    }
    public async Task<IEnumerable<string>> FindMissingPhotosInAzureAsync(HashSet<string> dbPhotoUrls, int batchSize)
    {
        var blobServiceClient = new BlobServiceClient(_blobStorageSettings.ConnectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_blobStorageSettings.ContainerName);

        var missingPhotos = new List<string>();

        // Fetch blobs from Azure Blob Storage in batches
        var azureBlobUrls = new HashSet<string>();

        await foreach (var blobItemPage in containerClient.GetBlobsAsync().AsPages(pageSizeHint: batchSize))
        {
            var currentBatch = blobItemPage.Values
                .Select(blobItem => containerClient.GetBlobClient(blobItem.Name).Uri.ToString());

            azureBlobUrls.UnionWith(currentBatch);
        }

        // Find the database URLs that are missing in Azure
        missingPhotos = dbPhotoUrls.Where(dbUrl => !azureBlobUrls.Contains(dbUrl)).ToList();

        logger.LogInformation("Found {MissingCount} photos missing from Azure Blob Storage.", missingPhotos.Count);

        return missingPhotos;
    }

    public async Task DeletePhotosInBatchesAsync(List<string> orphanedPhotos, int batchSize)
    {
        var blobServiceClient = new BlobServiceClient(_blobStorageSettings.ConnectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_blobStorageSettings.ContainerName);

        for (int i = 0; i < orphanedPhotos.Count; i += batchSize)
        {
            var currentBatch = orphanedPhotos.Skip(i).Take(batchSize).ToList();

            foreach (var photoUrl in currentBatch)
            {
                var blobName = GetBlobNameFromUri(photoUrl);
                var blobClient = containerClient.GetBlobClient(blobName);

                await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                logger.LogInformation("Deleted orphaned photo: {photoUrl}", photoUrl);
            }

            logger.LogInformation("Processed batch {BatchNumber}/{TotalBatches}", (i / batchSize) + 1, (orphanedPhotos.Count + batchSize - 1) / batchSize);
        }
    }
    private string GetBlobNameFromUri(string blobUri)
    {
        var uri = new Uri(blobUri);
        return Path.GetFileName(uri.LocalPath);
    }
}