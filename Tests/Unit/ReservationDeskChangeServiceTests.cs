using App;
using App.Areas.Locations;
using FluentAssertions;
using FluentResults;

namespace Tests;

public class ReservationDeskChangeServiceTests
{
    [Fact]
    public void DeskChange_ShouldFail_WhenFirstDeskHasNoReservationToTransfer()
    {
        ReservationDeskChangeService service = new();
        DateOnly currentDate = new(2018, 6, 13);
        Location location = new(5, 2);
        Desk oldDesk = new(location.Id);
        Desk newDesk = new(location.Id);

        Result result = service.ChangeDesk(oldDesk, newDesk, currentDate);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void DeskChange_ShouldFail_WhenHasReservationAndTryingToChangeItLessThan24HoursPrior()
    {
        ReservationDeskChangeService service = new();
        DateOnly currentDate = new(2018, 6, 13);
        DateOnly reservationDate = currentDate.AddDays(1);
        Location location = new(5, 2);
        Desk oldDesk = new(location.Id);
        Desk newDesk = new(location.Id);
        oldDesk.Reserve(
            reservationDate,
            currentDate
        );

        Result result = service.ChangeDesk(oldDesk, newDesk, currentDate);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void DeskChange_ShouldFail_WhenReservingNewDeskFailed()
    {
        ReservationDeskChangeService service = new();
        DateOnly currentDate = new(2018, 6, 13);
        DateOnly reservationDate = currentDate.AddDays(5);
        Location location = new(5, 2);
        Desk oldDesk = new(location.Id);
        Desk newDesk = new(location.Id);
        oldDesk.Reserve(
            reservationDate,
            currentDate
        );
        newDesk.Reserve(
            reservationDate,
            currentDate
        );

        Result result = service.ChangeDesk(oldDesk, newDesk, currentDate);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void DeskChange_ShouldRaiseDomainEvents_WhenSuccessful()
    {
        ReservationDeskChangeService service = new();
        DateOnly currentDate = new(2018, 6, 13);
        DateOnly reservationDate = currentDate.AddDays(5);
        Location location = new(5, 2);
        Desk oldDesk = new(location.Id);
        Desk newDesk = new(location.Id);
        oldDesk.Reserve(
            reservationDate,
            currentDate
        );

        Result result = service.ChangeDesk(oldDesk, newDesk, currentDate);

        result.IsSuccess.Should().BeTrue();
        oldDesk.DomainEvents.Should().HaveCount(3);
        newDesk.DomainEvents.Should().HaveCount(2);
    }
}
