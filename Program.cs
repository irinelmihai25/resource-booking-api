using ResourceBooking.Models;
using ResourceBooking.Requests;

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
        foreach(var item in bookings)
            if(item.Id==id)
             return Results.Ok(item);
             
    }
return Results.NotFound();
});
Console.WriteLine(booking.Overlaps(secondBooking));
var candidate=new Booking();
candidate.Id=3;
candidate.ResourceId=resource.Id;
candidate.Start=new DateTimeOffset(2026,9,20,10,30,0,TimeSpan.FromHours(3));
candidate.End=new DateTimeOffset(2026,9,20,11,30,0,TimeSpan.FromHours(3));
foreach(var i in bookings)
{
    if(candidate.Overlaps(i))
        Console.WriteLine(i.Id);
}
int nextBookingId=3;
app.MapPost("/bookings",(CreateBookingRequest request) =>
{   var newBooking=new Booking();
    newBooking.ResourceId=request.ResourceId;
    newBooking.Start=request.Start;
    newBooking.End=request.End;
    bool resourceExists=false;
    foreach(var item in resources)
    {
        if(newBooking.ResourceId==item.Id)
            resourceExists=true;
    }
    if(!resourceExists)
        return Results.BadRequest("Resursa nu exista.");
    if(newBooking.HasValidPeriod()==false)
        return Results.BadRequest("Sfarsitul trebuie sa fie dupa ora de inceput.");
    foreach(var item in bookings)
    {
        if(newBooking.Overlaps(item))
            return Results.Conflict("Resursa este deja rezervata in acest interval.");
    }
    
    newBooking.Id=nextBookingId;
    nextBookingId++;
    bookings.Add(newBooking);
    return Results.Created($"/bookings/{newBooking.Id}",newBooking);
}
);
int nextResourceId=3;
app.MapPost("/resources",(CreateResourceRequest request)=>
{   var newResource=new Resource();
    newResource.Name=request.Name;
    newResource.Description=request.Description;
    if(string.IsNullOrWhiteSpace(newResource.Name))
        return Results.BadRequest("Nu s a introdus numele");
    newResource.Id=nextResourceId;
    nextResourceId++;
    resources.Add(newResource);
    return Results.Created($"/resources/{newResource.Id}",newResource);
}
);
app.MapGet("/resources/{resourceId:int}/bookings",(int resourceId) =>{
    var matchingBookings=new List<Booking>();
    foreach(var item in bookings)
    {
        if (item.ResourceId == resourceId)
        {
            matchingBookings.Add(item);
        }
    }
    return Results.Ok(matchingBookings);

});
app.MapPost("/bookings/{id:int}/cancel",(int id)=>
{
    foreach(var item in bookings)
    {
        if (item.Id == id)
        {
            item.IsCancelled=true;
            return Results.Ok(item);
        }
    }
    return Results.NotFound("Rezervarea nu exista");
}
);
app.Run();
