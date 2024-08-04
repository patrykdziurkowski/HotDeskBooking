using App;
using Ductus.FluentDocker.Common;
using FluentAssertions;
using FluentResults;

namespace Tests;

public class DeskTests
{
    [Fact]
    public void Construction_ShouldRaiseDomainEvent()
    {
        Desk desk = new(Guid.NewGuid());

        desk.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void IsMadeUnavailableProperty_ShouldRaiseDomainEvent_WhenSetToTrue()
    {
        Desk desk = new(Guid.NewGuid());

        desk.IsMadeUnavailable = true;

        desk.DomainEvents.Should().HaveCount(2);
    }

    [Fact]
    public void IsMadeUnavailableProperty_ShouldRaiseDomainEvent_WhenSetToFalse()
    {
        Desk desk = new(Guid.NewGuid());

        desk.IsMadeUnavailable = false;

        desk.DomainEvents.Should().HaveCount(2);
    }

    [Fact]
    public void IsAvailable_ShouldReturnTrue_WhenDeskIsNotMadeUnavailableAndHasNoReservation()
    {
        Desk desk = new(Guid.NewGuid());
        DateOnly currentDate = new(2018, 6, 13);

        desk.IsAvailable(currentDate).Should().BeTrue();
    }

    [Fact]
    public void IsAvailable_ShouldReturnFalse_WhenDeskWasMadeUnavailable()
    {
        Desk desk = new(Guid.NewGuid());
        DateOnly currentDate = new(2018, 6, 13);

        desk.IsMadeUnavailable = true;

        desk.IsAvailable(currentDate).Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_ShouldReturnFalse_WhenDeskHasPendingReservation()
    {
        Desk desk = new(Guid.NewGuid());
        DateOnly currentDate = new(2018, 6, 13);
        DateOnly date = new(2018, 6, 24);

        desk.Reserve(date, currentDate);

        desk.IsAvailable(currentDate).Should().BeFalse();
    }

    [Fact]
    public void IsAvailable_ShouldReturnTrue_WhenPreviousReservationExpired()
    {
        Desk desk = new(Guid.NewGuid());
        DateOnly firstReservationDate = new(2018, 6, 13);
        DateOnly firstReservationStartDate = new(2018, 6, 20);
        DateOnly currentDate = new(2018, 6, 21);

        desk.Reserve(firstReservationStartDate, firstReservationDate);

        desk.IsAvailable(currentDate).Should().BeTrue();
    }

    [Fact]
    public void Reserve_ShouldRaiseDomainEvent_WhenDeskBooked()
    {
        Desk desk = new(Guid.NewGuid());
        DateOnly currentDate = new(2018, 6, 13);
        DateOnly date = new(2018, 6, 24);
        desk.Reserve(date, currentDate);

        desk.DomainEvents.Should().HaveCount(2);
    }

    [Fact]
    public void Reserve_ShouldReturnFail_WhenDeskIsNotAvailable()
    {
        Desk desk = new(Guid.NewGuid());
        DateOnly currentDate = new(2018, 6, 13);
        DateOnly date = new(2018, 6, 24);
        desk.Reserve(date, currentDate);

        Result result = desk.Reserve(date, currentDate);
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Reserve_ShouldReturnOk_WhenBooking7Days()
    {
        Desk desk = new(Guid.NewGuid());
        DateOnly currentDate = new(2018, 6, 13);
        DateOnly startDate = new(2018, 6, 20);
        DateOnly endDate = new(2018, 6, 26);

        Result result = desk.Reserve(startDate, endDate, currentDate);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Reserve_ShouldReturnFail_WhenBooking8Days()
    {
        Desk desk = new(Guid.NewGuid());
        DateOnly currentDate = new(2018, 6, 13);
        DateOnly startDate = new(2018, 6, 20);
        DateOnly endDate = new(2018, 6, 27);

        Result result = desk.Reserve(startDate, endDate, currentDate);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Reserve_ShouldReturnFail_WhenEndDateBeforeStartDate()
    {
        Desk desk = new(Guid.NewGuid());
        DateOnly currentDate = new(2018, 6, 13);
        DateOnly startDate = new(2018, 6, 20);
        DateOnly endDate = new(2018, 6, 19);

        Result result = desk.Reserve(startDate, endDate, currentDate);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Reserve_ShouldReturnOk_WhenBookingForADay()
    {
        Desk desk = new(Guid.NewGuid());
        DateOnly currentDate = new(2018, 6, 13);
        DateOnly startDate = new(2018, 6, 20);

        Result result = desk.Reserve(startDate, currentDate);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Reserve_ShouldReturnFail_WhenMakingReservationForThePast()
    {
        Desk desk = new(Guid.NewGuid());
        DateOnly currentDate = new(2018, 6, 21);
        DateOnly startDate = new(2018, 6, 20);

        Result result = desk.Reserve(startDate, currentDate);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void Reserve_ShouldReturnOk_WhenPreviousReservationExpired()
    {
        Desk desk = new(Guid.NewGuid());
        DateOnly firstReservationDate = new(2018, 6, 13);
        DateOnly firstReservationStartDate = new(2018, 6, 20);

        DateOnly secondReservationDate = new(2018, 6, 21);
        DateOnly secondReservationStartDate = new(2018, 6, 22);

        desk.Reserve(firstReservationStartDate, firstReservationDate);
        Result result = desk.Reserve(secondReservationStartDate, secondReservationDate);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Remove_ShouldReturnFail_WhenDeskHasNonExpiredReservation()
    {
        Desk desk = new(Guid.NewGuid());
        DateOnly currentDate = new(2018, 6, 13);
        DateOnly startDate = new(2018, 6, 20);
        desk.Reserve(startDate, currentDate);

        Result result = desk.Remove(currentDate);

        result.IsFailed.Should().BeTrue();
        desk.DomainEvents.Should().HaveCount(2);
    }

    [Fact]
    public void Remove_ShouldRaiseDomainEvent_WhenDeskRemoved()
    {
        Desk desk = new(Guid.NewGuid());
        DateOnly currentDate = new(2018, 6, 13);

        Result result = desk.Remove(currentDate);

        result.IsSuccess.Should().BeTrue();
        desk.DomainEvents.Should().HaveCount(2);
    }
}
