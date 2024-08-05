using App.Areas.Identity.Data;
using App.Areas.Locations;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;

namespace App;

public class LocationRepository : ILocationRepository
{
    private readonly ApplicationDbContext _context;

    public LocationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Location>> GetAsync()
    {
        List<Location> locations = await _context.Locations
            .Select(l => new Location(
                l.Id,
                l.BuildingNumber,
                l.Floor,
                _context.Desks
                    .Where(d => d.LocationId == l.Id)
                    .Select(d => d.Id)
                    .ToList()
            ))
            .ToListAsync();
            
        locations.ForEach(l => l.ClearDomainEvents());
        return locations;
    }

    public async Task<Location?> GetByIdAsync(Guid id)
    {
        Location? location = await _context.Locations
            .Where(l => l.Id == id)
            .Select(l => new Location(
                l.Id,
                l.BuildingNumber,
                l.Floor,
                _context.Desks
                    .Where(d => d.LocationId == l.Id)
                    .Select(d => d.Id)
                    .ToList()
            ))
            .FirstOrDefaultAsync();

        location?.ClearDomainEvents();
        return location;
    }

    public async Task SaveAsync(Location location)
    {
        foreach (DomainEvent e in location.DomainEvents)
        {
            if (e is LocationCreatedEvent)
            {
                await _context.Locations.AddAsync(location);
            }
            else if (e is LocationRemovedEvent)
            {
                _context.Locations.Remove(location);
            }
        }
        await _context.SaveChangesAsync();
        location.ClearDomainEvents();
    }
}
