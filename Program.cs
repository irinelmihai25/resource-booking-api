using ResourceBooking.Models;
using ResourceBooking.Requests;
using ResourceBooking.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString=builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
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
app.MapGet("/resources",async (AppDbContext db) => {
    var date = await db.Resources.ToListAsync();
    return Results.Ok(date);
    });
app.MapGet("/resources/{id:int}",async (int id,AppDbContext db) => {
var item = await db.Resources.FindAsync(id);

    if(item!=null)
        return Results.Ok(item);

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
app.MapGet("/bookings",async (AppDbContext db) =>
{
    var date=await db.Bookings.ToListAsync();
    return Results.Ok(date);
});
app.MapGet("/bookings/{id:int}",async (int id,AppDbContext db) =>
{
    
        var item=await db.Bookings.FindAsync(id);
            if(item!=null)
             return Results.Ok(item);
             
    
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

app.MapPost("/bookings",async (CreateBookingRequest request,AppDbContext db) =>
{   var newBooking=new Booking();
    newBooking.ResourceId=request.ResourceId;
    newBooking.Start=request.Start.ToUniversalTime();
    newBooking.End=request.End.ToUniversalTime();
    
    var resourceExists= await db.Resources.AnyAsync(r=>r.Id==newBooking.ResourceId);

    if(!resourceExists)
        return Results.BadRequest("Resursa nu exista.");
    if(newBooking.HasValidPeriod()==false)
        return Results.BadRequest("Sfarsitul trebuie sa fie dupa ora de inceput.");
    var existingBookings = await db.Bookings.Where(b => b.ResourceId == newBooking.ResourceId && !b.IsCancelled).ToListAsync();
    foreach(var item in existingBookings)
    {
        if(newBooking.Overlaps(item))
            return Results.Conflict("Resursa este deja rezervata in acest interval.");
    }
    
    
    db.Bookings.Add(newBooking);
        await db.SaveChangesAsync();
        return Results.Created($"/bookings/{newBooking.Id}", newBooking);
}
);

app.MapPost("/resources",async (CreateResourceRequest request,AppDbContext db)=>
{   var newResource=new Resource();
    newResource.Name=request.Name;
    newResource.Description=request.Description;
    if(string.IsNullOrWhiteSpace(newResource.Name))
        return Results.BadRequest("Nu s a introdus numele");
    db.Resources.Add(newResource);
    await db.SaveChangesAsync();
    return Results.Created($"/resources/{newResource.Id}",newResource);
}
);
app.MapGet("/resources/{resourceId:int}/bookings",async (int resourceId,AppDbContext db) =>{
    var matching=await db.Bookings.Where(b => b.ResourceId==resourceId).ToListAsync();
   
    return Results.Ok(matching);

});
app.MapPost("/bookings/{id:int}/cancel",async (int id,AppDbContext db)=>
{   
    var exists=await db.Bookings.FindAsync(id);
    if(exists==null)
        return Results.NotFound("Rezervarea nu exista");
    exists.IsCancelled=true;
    await db.SaveChangesAsync();
    return Results.Ok(exists);
}
);
app.Run();
