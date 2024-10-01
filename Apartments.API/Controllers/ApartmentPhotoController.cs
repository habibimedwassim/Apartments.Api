using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentPhotoDtos;
using Apartments.Application.IServices;
using Apartments.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apartments.API.Controllers;

[ApiController]
[Route("api/apartments/{apartmentId:int}/photos")]
[Authorize]
public class ApartmentPhotoController(IApartmentPhotoService apartmentPhotoService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetApartmentPhotos([FromRoute] int apartmentId)
    {
        var result = await apartmentPhotoService.GetApartmentPhotos(apartmentId);
        return Ok(result.Data);
    }

    [HttpGet("{photoId:int}")]
    public async Task<IActionResult> GetApartmentPhotoById([FromRoute] int apartmentId, [FromRoute] int photoId)
    {
        var result = await apartmentPhotoService.GetApartmentPhotoById(photoId, apartmentId);
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> AddPhotosToApartment([FromRoute] int apartmentId,
        [FromForm] UploadApartmentPhotoDto uploadApartmentPhoto)
    {
        uploadApartmentPhoto.ApartmentId = apartmentId;
        var result = await apartmentPhotoService.AddPhotosToApartment(uploadApartmentPhoto);

        if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));

        return Ok(result.Data);
    }

    [HttpDelete("{photoId:int}")]
    public async Task<IActionResult> DeletePhotoFromApartment([FromRoute] int apartmentId, [FromRoute] int photoId)
    {
        var result = await apartmentPhotoService.DeletePhotoFromApartment(photoId, apartmentId);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }
}