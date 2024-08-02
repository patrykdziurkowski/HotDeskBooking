using App.Areas.Identity.Data;
using App.Areas.Locations;
using NuGet.Protocol.Core.Types;

namespace App;

public class LocationRepository : ILocationRepository
{
    private readonly ApplicationDbContext _context;

    public LocationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Location> Get()
    {
        return _context.Locations.ToList();
    }

    public async Task Save(Location location)
    {
        foreach (DomainEvent e in location.DomainEvents)
        {
            if (e is LocationCreatedEvent)
            {
                await _context.Locations.AddAsync(location);
            }
            if (e is LocationRemovedEvent)
            {
                _context.Locations.Remove(location);
            }
        }
        await _context.SaveChangesAsync();
        location.ClearDomainEvents();
    }
}
