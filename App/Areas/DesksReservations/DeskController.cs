using App.Areas.Locations;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace App;

public class DeskController : Controller
{
    private readonly IDeskRepository _deskRepository;
    private readonly ILocationRepository _locationRepository;

    public DeskController(
        IDeskRepository deskRepository,
        ILocationRepository locationRepository)
    {
        _deskRepository = deskRepository;
        _locationRepository = locationRepository;
    }

    [HttpGet]
    [Route("/Locations/{locationId}/Desks")]
    public async Task<IActionResult> GetAll(Guid locationId)
    {
        Location? location = await _locationRepository.GetByIdAsync(locationId);
        if (location is null)
        {
            return NotFound();
        }

        List<Desk> desks =  await _deskRepository.GetByLocationAsync(locationId);
        List<DeskDto> deskDtos = desks.Select(d => DeskDto.FromDesk(d)).ToList();
        return Ok(deskDtos);
    }

    [HttpPost]
    [Route("/Locations/{locationId}/Desks")]
    public async Task<IActionResult> Create(Guid locationId)
    {
        Location? location = await _locationRepository.GetByIdAsync(locationId);
        if (location is null)
        {
            return NotFound();
        }

        Desk desk = new(locationId);
        await _deskRepository.SaveAsync(desk);
        return StatusCode(201);
    }

    [HttpDelete]
    [Route("/Locations/{locationId}/Desks/{deskId}")]
    public async Task<IActionResult> Remove(
        Guid locationId,
        Guid deskId
    )
    {
        Location? location = await _locationRepository.GetByIdAsync(locationId);
        if (location is null)
        {
            return NotFound();
        }

        Desk? desk = (await _deskRepository.GetByLocationAsync(locationId))
            .FirstOrDefault(d => d.Id == deskId);
        if (desk is null)
        {
            return NotFound();
        }

        Result result = desk.Remove(DateOnly.FromDateTime(DateTime.Now));
        if (result.IsFailed)
        {
            return StatusCode(403, result.Errors.First().Message);
        }

        await _deskRepository.SaveAsync(desk);
        return Ok();
    }

    [HttpPatch]
    [Route("/Locations/{locationId}/Desks/{deskId}")]
    public async Task<IActionResult> MakeUnavailable(
        Guid locationId,
        Guid deskId,
        bool isMadeUnavailable)
    {
        Location? location = await _locationRepository.GetByIdAsync(locationId);
        if (location is null)
        {
            return NotFound();
        }

        Desk? desk = (await _deskRepository.GetByLocationAsync(locationId))
            .FirstOrDefault(d => d.Id == deskId);
        if (desk is null)
        {
            return NotFound();
        }
        
        desk.IsMadeUnavailable = isMadeUnavailable;
        await _deskRepository.SaveAsync(desk);
        return Ok();
        
    }
}
