using FluentResults;

namespace App;

public class ReservationDeskChangeService
{
    public Result ChangeDesk(Desk oldDesk, Desk newDesk, DateOnly currentDate)
    {
        if (oldDesk.Reservation is null)
        {
            return Result.Fail("The desk has no reservation to transfer.");
        }

        if (currentDate.AddDays(1) >= oldDesk.Reservation.StartDate)
        {
            return Result.Fail("Cannot change the reservation less than a day prior to it.");
        }

        Result result = newDesk.Reserve(
            oldDesk.Reservation.StartDate,
            oldDesk.Reservation.EndDate,
            currentDate
        );
        if (result.IsFailed)
        {
            return result;
        }
        oldDesk.CancelReservation();
        return Result.Ok();
    }
}
