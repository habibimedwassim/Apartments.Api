using Microsoft.AspNetCore.Mvc;
using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Application.DTOs.ApartmentPhotoDTOs;
using RenTN.Application.Services.ApartmentPhotosService;

namespace RenTN.API.Controllers;

[ApiController]
[Route("api/apartments/{apartmentId}/photos")]
public class ApartmentPhotosController(IApartmentPhotosService _apartmentPhotosService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllApartmentPhotos([FromRoute] int apartmentId)
    {
        var apartmentPhotos = await _apartmentPhotosService.GetAllApartmentPhotos(apartmentId);
        return Ok(apartmentPhotos);
    }

    [HttpGet("{photoId:int}")]
    public async Task<ActionResult<ApartmentDTO>> GetByIdForApartment([FromRoute] int apartmentId, [FromRoute] int photoId)
    {
        var apartmentPhoto = await _apartmentPhotosService.GetByIdForApartment(apartmentId, photoId);
        return Ok(apartmentPhoto);
    }

    [HttpPost]
    public async Task<IActionResult> AddApartmentPhoto([FromRoute] int apartmentId, [FromBody] CreateUpdateApartmentPhotoDTO createApartmentPhotoDTO)
    {
        await _apartmentPhotosService.AddApartmentPhoto(apartmentId, createApartmentPhotoDTO);
        return NoContent();
    }

    [HttpPatch("{photoId:int}")]
    public async Task<IActionResult> UpdateApartmentPhoto([FromRoute] int apartmentId, [FromRoute] int photoId, [FromBody] ApartmentPhotoDTO updateApartmentPhotoDTO)
    {
        updateApartmentPhotoDTO.ID = photoId;
        await _apartmentPhotosService.UpdateApartmentPhoto(apartmentId, updateApartmentPhotoDTO);
        return NoContent();
    }

    [HttpDelete("{photoId:int}")]
    public async Task<IActionResult> DeleteApartmentPhoto([FromRoute] int apartmentId, [FromRoute] int photoId)
    {
        await _apartmentPhotosService.DeleteApartmentPhoto(apartmentId, photoId);
        return NoContent();
    }
}
