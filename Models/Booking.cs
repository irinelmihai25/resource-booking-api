namespace ResourceBooking.Models;
public class Booking
{
    public int Id { get;set; }
    public int ResourceId { get;set; }
    public DateTimeOffset Start { get;set; }
    public DateTimeOffset End { get;set ; }

    public bool HasValidPeriod()
    {
        if(Start<End)
            return true;
        else
            return false;
    }
    
}