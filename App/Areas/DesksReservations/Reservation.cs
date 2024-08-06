namespace App;

public class Reservation
{
    public Guid Id { get; }
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }
    public Guid EmployeeId { get; }

    public Reservation(
        DateOnly startDate,
        DateOnly endDate,
        Guid employeeId
    )
    {
        Id = Guid.NewGuid();
        StartDate = startDate;
        EndDate = endDate;
        EmployeeId = employeeId;
    }

    // EF
    private Reservation()
    {
        Id = default!;
        StartDate = default!;
        EndDate = default!;
        EmployeeId = default!;
    }
}
