using ResourceBooking.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
var resource=new Resource();
resource.Id=1;
resource.Name="Sala B";
resource.Description="Etajul 1,cu proiector";
Console.WriteLine(resource.Name);
app.MapGet("/resource",() => resource);
var booking=new Booking();
booking.Id=1;
booking.ResourceId=resource.Id;
booking.Start=new DateTimeOffset(2026,9,19,23,0,0,TimeSpan.FromHours(3));
booking.End=new DateTimeOffset(2026,9,20,1,0,0,TimeSpan.FromHours(3));
Console.WriteLine(booking.HasValidPeriod());
var projector=new Resource();
projector.Id=2;
projector.Name="Proiector";
projector.Description="Descriere";
var resources=new List<Resource>();
resources.Add(resource);
resources.Add(projector);
app.MapGet("/resources",() => resources);
app.MapGet("/resources/{id:int}",(int id) => {
foreach (var item in resources)
{
    if(item.Id==id)
        return Results.Ok(item);
}
return Results.NotFound();
});
var secondBooking=new Booking();
secondBooking.Id=2;
secondBooking.ResourceId=resource.Id;
secondBooking.Start=new DateTimeOffset(2026,9,20,10,0,0,TimeSpan.FromHours(3));
secondBooking.End=new DateTimeOffset(2026,9,20,11,0,0,TimeSpan.FromHours(3));
var bookings=new List<Booking>();
bookings.Add(booking);
bookings.Add(secondBooking);
app.MapGet("/bookings",() => bookings);
app.MapGet("/bookings/{id:int}",(int id) =>
{
    {
        foreach(var bitem in bookings)
            if(bitem.Id==id)
             return Results.Ok(bitem);
             
    }
return Results.NotFound();
});
Console.WriteLine(booking.Overlaps(secondBooking));
app.Run();
