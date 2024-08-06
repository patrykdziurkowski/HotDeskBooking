namespace App;

public class ReservationDto
{
    public Guid Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public Guid? EmployeeId { get; set; }

    public static ReservationDto? FromReservation(Reservation? reservation)
    {
        if (reservation is null)
        {
            return null;
        }
        
        return new ReservationDto()
        {
            Id = reservation.Id,
            StartDate = reservation.StartDate,
            EndDate = reservation.EndDate,
            EmployeeId = reservation.EmployeeId
        };
    }
}
