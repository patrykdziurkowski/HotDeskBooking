using System.Formats.Asn1;
using App.Areas.Locations;
using FluentResults;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App;

[Authorize]
public class LocationController : Controller
{
    private ILocationRepository _locationRepository;
    private AddLocationModelValidator _addValidator = new();

    public LocationController(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    [HttpGet]
    [Route("/Locations")]
    public async Task<IActionResult> GetAll()
    {
        List<Location> locations = await _locationRepository.GetAsync();
        List<LocationDto> locationDtos = locations
            .Select(l => LocationDto.FromLocation(l))
            .ToList();
        return Ok(locations);
    }

    [HttpPost]
    [Route("/Locations")]
    public async Task<IActionResult> Add(AddLocationModel input)
    {
        ValidationResult result = await _addValidator.ValidateAsync(input);
        if (result.IsValid == false)
        {
            return BadRequest(result.Errors);
        }

        Location location = new((int) input.BuildingNumber!, (int) input.Floor!);
        await _locationRepository.SaveAsync(location);
        return StatusCode(201);
    }

    [HttpDelete]
    [Route("/Locations/{id}")]
    public async Task<IActionResult> Remove(Guid id)
    {
        Location? location = await _locationRepository.GetByIdAsync(id);
        if (location is null)
        {
            return NotFound("No location with matching id was found.");
        }

        Result result = location.Remove();
        if (result.IsFailed)
        {
            return StatusCode(403, result.Errors);
        }

        await _locationRepository.SaveAsync(location);
        return Ok();
    }
}
