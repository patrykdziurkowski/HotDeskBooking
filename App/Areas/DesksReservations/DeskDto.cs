namespace App;

public class DeskDto
{
    public Guid Id { get; set; }
    public bool IsMadeUnavailable { get; set; }
    public Guid LocationId { get; set; }
    public ReservationDto? Reservation { get; set; }

    public static DeskDto FromDesk(Desk desk)
    {
        return new DeskDto()
        {
            Id = desk.Id,
            IsMadeUnavailable = desk.IsMadeUnavailable,
            LocationId = desk.LocationId,
            Reservation = ReservationDto.FromReservation(desk.Reservation)
        };
    }
}
