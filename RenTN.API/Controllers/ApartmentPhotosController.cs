using Microsoft.AspNetCore.Mvc;
using RenTN.Application.DTOs.ApartmentDTOs;
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

    [HttpGet("{id}")]
    public async Task<ActionResult<ApartmentDTO>> GetByIdForApartment([FromRoute] int apartmentId, [FromRoute] int id)
    {
        var apartmentPhoto = await _apartmentPhotosService.GetByIdForApartment(apartmentId, id);
        return Ok(apartmentPhoto);
    }
}
