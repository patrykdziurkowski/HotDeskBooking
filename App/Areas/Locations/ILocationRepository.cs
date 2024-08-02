using App.Areas.Locations;

namespace App;

public interface ILocationRepository
{
    public List<Location> Get();
    public Task Save(Location location);
}
