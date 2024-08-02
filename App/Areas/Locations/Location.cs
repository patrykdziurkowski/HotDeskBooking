using FluentResults;
using Microsoft.IdentityModel.Tokens;

namespace App.Areas.Locations;

public class Location : AggreggateRoot
{
    public Guid Id { get; }
    public int BuildingNumber { get; }
    public int Floor { get; }

    public IEnumerable<Guid> Desks => _desks;
    private List<Guid> _desks;

    public Location(
        Guid id,
        int buildingNumber,
        int floor,
        List<Guid> desks
    )
    {
        Id = id;
        BuildingNumber = buildingNumber;
        Floor = floor;
        _desks = desks;
        RaiseDomainEvent(new LocationCreatedEvent());
    }

    public Location(int buildingNumber, int floor)
        : this(
            Guid.NewGuid(),
            buildingNumber,
            floor,
            []
        )
    {
    }

    // EF
    public Location()
    {
        Id = default!;
        BuildingNumber = default!;
        Floor = default!;
        _desks = [];
    }

    public Result Remove()
    {
        if (_desks.IsNullOrEmpty() == false)
        {
            return Result.Fail("In order to be removed, this location musn't contain any desks.");
        }

        RaiseDomainEvent(new LocationRemovedEvent());
        return Result.Ok();
    }
}
