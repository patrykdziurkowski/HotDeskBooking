using App.Areas.Location;
using FluentAssertions;
using FluentResults;

namespace Tests;

public class LocationTests
{
    [Fact]
    public void Constructor_ShouldRaiseDomainEvent()
    {
        Location location = new(1, 2);

        location.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Remove_ShouldFail_WhenLocationHasDesks()
    {
        Location location = new(
            Guid.NewGuid(),
            1,
            2,
            [Guid.NewGuid()]);
        
        Result result = location.Remove();

        result.IsFailed.Should().BeTrue();
        location.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Remove_ShouldReturnSuccessAndRaiseDomainEvent_WhenLocationHasNoDesks()
    {
        Location location = new(
            Guid.NewGuid(),
            1,
            2,
            []);
        
        Result result = location.Remove();

        result.IsSuccess.Should().BeTrue();
        location.DomainEvents.Should().HaveCount(2);
    }
}
