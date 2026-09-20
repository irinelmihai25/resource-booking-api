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
app.Run();
