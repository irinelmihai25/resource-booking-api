namespace ResourceBooking.Requests;
public class CreateBookingRequest
{
    public int ResourceId{get;set;}
    public DateTimeOffset Start {get;set;}

    public DateTimeOffset End {get;set;}
}