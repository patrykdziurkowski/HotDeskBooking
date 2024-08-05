using App.Areas.Identity.Data;
using App.Areas.Locations;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.DesksReservations;

public class DeskRepository : IDeskRepository
{
    private readonly ApplicationDbContext _dbContext;
    public DeskRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Desk>> GetAsync()
    {
        return await _dbContext.Desks
            .Include(d => d.Reservation)
            .ToListAsync();
    }

    public async Task<Desk?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Desks
            .Include(d => d.Reservation)
            .Where(d => d.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Desk>> GetByLocationAsync(Guid locationId)
    {
        return await _dbContext.Desks
            .Include(d => d.Reservation)
            .Where(d => d.LocationId == locationId)
            .ToListAsync();
    }

    public async Task SaveAsync(Desk desk)
    {
        foreach (DomainEvent e in desk.DomainEvents)
        {
            if (e is DeskAddedEvent)
            {
                await _dbContext.Desks.AddAsync(desk);
            }
            else if (e is DeskReservedEvent)
            {
                if (desk.Reservation is null) continue;
                await _dbContext.Reservations.AddAsync(desk.Reservation);
            }
            else if (e is DeskRemovedEvent)
            {
                _dbContext.Desks.Remove(desk);
            }
        }
        
        await _dbContext.SaveChangesAsync();
        desk.ClearDomainEvents();
    }
}
