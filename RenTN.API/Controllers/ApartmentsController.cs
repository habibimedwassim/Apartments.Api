using Microsoft.AspNetCore.Mvc;
using RenTN.API.Utilities;
using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Application.Services.ApartmentsService;

namespace RenTN.API.Controllers;

[ApiController]
[Route("api/apartments")]
public class ApartmentsController(IApartmentsService _apartmentsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllApartments()
    {
        var result = await _apartmentsService.GetApartments();

        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetApartmentByID([FromRoute] int id)
    {
        var result = await _apartmentsService.GetApartmentByID(id);

        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateApartment([FromForm] CreateApartmentDTO createApartmentDTO)
    {
        var result = await _apartmentsService.CreateApartmentWithPhotosAsync(createApartmentDTO);

        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateApartment([FromRoute] int id, [FromBody] UpdateApartmentDTO updateApartmentDTO)
    {
        updateApartmentDTO.ID = id;
        var result = await _apartmentsService.UpdateApartment(updateApartmentDTO);

        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApartment([FromRoute] int id)
    {
        var result = await _apartmentsService.DeleteApartment(id);

        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }
}
