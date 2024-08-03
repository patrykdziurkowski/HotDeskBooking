using App.Areas.Locations;

namespace App;

public class LocationDto
{
    public Guid Id { get; set; }
    public int BuildingNumber { get; set; }
    public int Floor { get; set; }
    public List<Guid> Desks { get; set; } = [];

    public static LocationDto FromLocation(Location location)
    {
        return new LocationDto()
        {
            Id = location.Id,
            BuildingNumber = location.BuildingNumber,
            Floor = location.Floor,
            Desks = location.Desks.ToList()
        };
    }
}
