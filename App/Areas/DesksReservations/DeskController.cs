using App.Areas.Locations;
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
    [Route("/Location/{locationId}/Desk")]
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
    [Route("/Location/{locationId}/Desk")]
    public async Task<IActionResult> Create(Guid locationId)
    {
        Desk desk = new(locationId);
        await _deskRepository.SaveAsync(desk);
        return StatusCode(201);
    }
}
