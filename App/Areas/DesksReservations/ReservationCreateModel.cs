namespace App;

public class ReservationCreateModel
{
    public Guid? DeskId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}
