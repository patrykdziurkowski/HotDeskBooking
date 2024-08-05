namespace App;

public class Reservation
{
    public Guid Id { get; }
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }

    public Reservation(
        DateOnly startDate,
        DateOnly endDate
    )
    {
        Id = Guid.NewGuid();
        StartDate = startDate;
        EndDate = endDate;
    }

    // EF
    private Reservation()
    {
        Id = default!;
        StartDate = default!;
        EndDate = default!;
    }
}
