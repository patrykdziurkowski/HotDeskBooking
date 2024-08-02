using App.Areas.Locations;

namespace App;

public interface ILocationRepository
{
    public Task<List<Location>> GetAsync();
    public Task<Location?> GetByIdAsync(Guid id);
    public Task SaveAsync(Location location);
}
