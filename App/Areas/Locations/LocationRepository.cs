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
        return await _context.Locations.ToListAsync();
    }

    public async Task<Location?> GetByIdAsync(Guid id)
    {
        return await _context.Locations.FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task SaveAsync(Location location)
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
