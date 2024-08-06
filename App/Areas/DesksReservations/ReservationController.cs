using System.Security.Claims;
using App.Areas.DesksReservations;
using App.Areas.Locations;
using FluentResults;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace App;

public class ReservationController : Controller
{
    private readonly IDeskRepository _deskRepository;
    private readonly ILocationRepository _locationRepository;
    private ReservationCreateModelValidator _createValidator = new();

    public ReservationController(
        IDeskRepository deskRepository,
        ILocationRepository locationRepository)
    {
        _deskRepository = deskRepository;
        _locationRepository = locationRepository;
    }

    [HttpGet]
    [Route("/Locations/{locationId}/Desks/{deskId}/Reservation")]
    public async Task<IActionResult> Get(Guid locationId, Guid deskId)
    {
        Location? location = await _locationRepository.GetByIdAsync(locationId);
        if (location is null)
        {
            return NotFound();
        }

        Desk? desk = (await _deskRepository.GetByLocationAsync(locationId))
            .FirstOrDefault(d => d.Id == deskId);
        if (desk is null || desk.Reservation is null)
        {
            return NotFound();
        }
        
        return Ok(desk.Reservation);
    }

    [HttpPost]
    [Route("/Locations/{locationId}/Desks/{deskId}/Reservation")]
    public async Task<IActionResult> Create(ReservationCreateModel model)
    {
        ValidationResult validationResult = _createValidator.Validate(model);
        if (validationResult.IsValid == false)
        {
            return BadRequest();
        }

        Desk? desk = await _deskRepository.GetByIdAsync(model.DeskId!.Value);
        if (desk is null)
        {
            return NotFound();
        }

        Guid? employeeId = GetEmployeeId();
        if (employeeId is null)
        {
            return NotFound();
        }

        Result result = desk.Reserve(
            model.StartDate!.Value, 
            model.EndDate!.Value, 
            DateOnly.FromDateTime(DateTime.Now),
            employeeId.Value);
        if (result.IsFailed)
        {
            return StatusCode(403, result.Errors.First().Message);
        }
        await _deskRepository.SaveAsync(desk);
        return Ok();
    }

    [HttpPatch]
    [Route("/Locations/{locationId}/Desks/{oldDeskId}/Reservation")]
    public async Task<IActionResult> ChangeDesk(
        Guid locationId,
        Guid oldDeskId,
        Guid newDeskId
    )
    {
        Location? location = await _locationRepository.GetByIdAsync(locationId);
        if (location is null)
        {
            return NotFound();
        }

        Desk? oldDesk = await _deskRepository.GetByIdAsync(oldDeskId);
        if (oldDesk is null)
        {
            return NotFound();
        }

        Desk? newDesk = await _deskRepository.GetByIdAsync(newDeskId);
        if (newDesk is null)
        {
            return NotFound();
        }

        ReservationDeskChangeService service = new();
        Result result = service.ChangeDesk(oldDesk, newDesk, DateOnly.FromDateTime(DateTime.Now));
        if (result.IsFailed)
        {
            return StatusCode(403, result.Errors.First().Message);
        }
        await _deskRepository.SaveAsync([oldDesk, newDesk]);
        return Ok();
    }


    private Guid? GetEmployeeId()
    {
        string? identifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (identifier is null)
        {
            return null;
        }
        return Guid.Parse(identifier);
    }
}
