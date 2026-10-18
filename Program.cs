using ResourceBooking.Models;
using ResourceBooking.Requests;
using ResourceBooking.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

// Resources endpoints
app.MapGet("/resources", async (AppDbContext db) =>
{
    var data = await db.Resources.ToListAsync();
    return Results.Ok(data);
});

app.MapGet("/resources/{id:int}", async (int id, AppDbContext db) =>
{
    var item = await db.Resources.FindAsync(id);
    if (item != null)
        return Results.Ok(item);

    return Results.NotFound();
});

app.MapPost("/resources", async (CreateResourceRequest request, AppDbContext db) =>
{
    var newResource = new Resource();
    newResource.Name = request.Name;
    newResource.Description = request.Description;
    if (string.IsNullOrWhiteSpace(newResource.Name))
        return Results.BadRequest("Nu s a introdus numele");

    db.Resources.Add(newResource);
    await db.SaveChangesAsync();
    return Results.Created($"/resources/{newResource.Id}", newResource);
});

app.MapGet("/resources/{resourceId:int}/bookings", async (int resourceId, AppDbContext db) =>
{
    var matching = await db.Bookings.Where(b => b.ResourceId == resourceId).ToListAsync();
    return Results.Ok(matching);
});

// Bookings endpoints
app.MapGet("/bookings", async (AppDbContext db) =>
{
    var data = await db.Bookings.ToListAsync();
    return Results.Ok(data);
});

app.MapGet("/bookings/{id:int}", async (int id, AppDbContext db) =>
{
    var item = await db.Bookings.FindAsync(id);
    if (item != null)
        return Results.Ok(item);

    return Results.NotFound();
});

app.MapPost("/bookings", async (CreateBookingRequest request, AppDbContext db) =>
{
    var newBooking = new Booking();
    newBooking.ResourceId = request.ResourceId;
    newBooking.Start = request.Start.ToUniversalTime();
    newBooking.End = request.End.ToUniversalTime();

    var resourceExists = await db.Resources.AnyAsync(r => r.Id == newBooking.ResourceId);
    if (!resourceExists)
        return Results.BadRequest("Resursa nu exista.");

    if (newBooking.HasValidPeriod() == false)
        return Results.BadRequest("Sfarsitul trebuie sa fie dupa ora de inceput.");

    var existingBookings = await db.Bookings.Where(b => b.ResourceId == newBooking.ResourceId && !b.IsCancelled).ToListAsync();
    foreach (var item in existingBookings)
    {
        if (newBooking.Overlaps(item))
            return Results.Conflict("Resursa este deja rezervata in acest interval.");
    }

    db.Bookings.Add(newBooking);
    await db.SaveChangesAsync();
    return Results.Created($"/bookings/{newBooking.Id}", newBooking);
});

app.MapPost("/bookings/{id:int}/cancel", async (int id, AppDbContext db) =>
{
    var exists = await db.Bookings.FindAsync(id);
    if (exists == null)
        return Results.NotFound("Rezervarea nu exista");

    if (exists.IsCancelled)
        return Results.BadRequest("Rezervarea este deja anulată.");

    exists.IsCancelled = true;
    await db.SaveChangesAsync();
    return Results.Ok(exists);
});

app.Run();
