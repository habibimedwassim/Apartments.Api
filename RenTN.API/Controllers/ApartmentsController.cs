using Microsoft.AspNetCore.Mvc;
using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Application.Services.ApartmentsService;
using RenTN.Domain.Entities;

namespace RenTN.API.Controllers;

[ApiController]
[Route("api/apartments")]
public class ApartmentsController(IApartmentsService _apartmentsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var apartments = await _apartmentsService.GetApartments();
        return Ok(apartments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByID([FromRoute] int id)
    {
        var apartment = await _apartmentsService.GetApartmentByID(id);

        if (apartment == null) return NotFound();

        return Ok(apartment);
    }

    [HttpPost]
    public async Task<IActionResult> CreateApartment([FromBody] CreateApartmentDTO createApartmentDTO)
    {
        var id = await _apartmentsService.CreateApartment(createApartmentDTO);

        return CreatedAtAction(nameof(GetByID), new {id}, null);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateApartment([FromRoute] int id, [FromBody] UpdateApartmentDTO updateApartmentDTO)
    {
        updateApartmentDTO.ID = id;
        await _apartmentsService.UpdateApartment(updateApartmentDTO);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApartment([FromRoute] int id)
    {
        await _apartmentsService.DeleteApartment(id);

        return NoContent();
    }
}
