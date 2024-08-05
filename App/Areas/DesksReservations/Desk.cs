using FluentResults;

namespace App;

public class Desk : AggreggateRoot
{
    public Guid Id { get; }
    public bool IsMadeUnavailable
    { 
        get => _isMadeUnavailable;
        set
        {
            _isMadeUnavailable = value;
            RaiseDomainEvent(new DeskAvailabilityChangedEvent());
        }    
    }
    private bool _isMadeUnavailable;
    public Guid LocationId { get; }
    public Guid? ReservationId { get; }
    public Reservation? Reservation
    {
        get => _reservation; 
        private set
        {
            _reservation = value;
            RaiseDomainEvent(new DeskReservedEvent());
        } 
    }
    private Reservation? _reservation;

    public Desk(
        Guid id,
        Guid locationId
    )
    {
        Id = id;
        LocationId = locationId;
        _isMadeUnavailable = false;
        RaiseDomainEvent(new DeskAddedEvent());
    }

    public Desk(Guid locationId)
        : this(Guid.NewGuid(), locationId)
    { }

    // EF
    private Desk()
    {
        Id = default!;
        LocationId = default!;
        _isMadeUnavailable = default!;
    }

    public bool IsAvailable(DateOnly currentDate)
    {
        if (IsMadeUnavailable)
        {
            return false;
        }

        if (Reservation is null)
        {
            return true;
        }

        return currentDate > Reservation.EndDate;
    }

    public Result Reserve(DateOnly startDate, DateOnly endDate, DateOnly currentDate)
    {
        if (currentDate > startDate)
        {
            return Result.Fail("Reservation cannot be in the past.");
        }

        if (endDate < startDate)
        {
            return Result.Fail("Reservation's end date must come after the start date.");
        }

        if (startDate.AddDays(6) < endDate)
        {
            return Result.Fail("Cannot book a desk for more than a week.");
        }

        if (HasPendingReservation(currentDate))
        {
            return Result.Fail("The chosen desk has already been booked.");
        }

        Reservation = new Reservation(startDate, endDate);
        return Result.Ok();
    }

    public Result Reserve(DateOnly startDate, DateOnly currentDate)
    {
        return Reserve(startDate, startDate, currentDate);
    }

    public Result Remove(DateOnly currentDate)
    {
        if (HasPendingReservation(currentDate))
        {
            return Result.Fail("Cannot remove a desk that has a pending reservation.");
        }
        
        RaiseDomainEvent(new DeskRemovedEvent());
        return Result.Ok();
    }

    private bool HasPendingReservation(DateOnly currentDate)
    {
        return (Reservation is not null) && (currentDate < Reservation.EndDate);
    }
}
