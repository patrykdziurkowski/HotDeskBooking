using App.Areas.Locations;

namespace App;

public interface IDeskRepository
{
    public Task<List<Desk>> GetAsync();
    public Task<List<Desk>> GetByLocationAsync(Guid locationId);
    public Task<Desk?> GetByIdAsync(Guid id);
    public Task SaveAsync(Desk desk);
    public Task SaveAsync(List<Desk> desks);
}
