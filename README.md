# Resource Booking API

A RESTful web API for managing shared resources (meeting rooms, equipment, workspaces) and scheduling reservations with conflict-prevention business logic, backed by PostgreSQL and Entity Framework Core.

Built with **C#** and **.NET 10** using **ASP.NET Core Minimal APIs**, verified with an **xUnit** test suite.

---

## Architecture and Key Capabilities

* **Resource Lifecycle Management:**
  * Endpoints to create, retrieve, and enumerate bookable assets.
  * Input validation ensuring non-empty resource naming and proper state initialization.
* **Reservation Lifecycle and Conflict Prevention:**
  * Reservation scheduling backed by interval ordering verification (`Start < End`).
  * Automatic normalization to UTC (`DateTimeOffset.ToUniversalTime()`) to ensure consistent temporal evaluation across timezones.
  * Interval overlap detection preventing double-booking of any active resource.
  * Explicit cancellation mechanism freeing up previously reserved time slots for new bookings while guarding against redundant cancellation calls.
* **Persistence Layer:**
  * Relational database persistence using **PostgreSQL** and **Entity Framework Core**.
  * Code-first schema migrations (`InitialCreate`).
  * Fully asynchronous database interactions (`ToListAsync`, `FindAsync`, `AnyAsync`, `SaveChangesAsync`).
* **Automated Unit Testing:**
  * Comprehensive test suite built with **xUnit** isolating the domain model logic.
  * Verified scenarios include valid intervals, inverted intervals, zero-duration reservations, partial intersections, adjacent intervals (seamless back-to-back bookings), and post-cancellation slot reusability.

---

## Tech Stack

* **Language:** C# 13 / .NET 10
* **Framework:** ASP.NET Core Minimal APIs
* **ORM:** Entity Framework Core 10 (Npgsql.EntityFrameworkCore.PostgreSQL)
* **Database Engine:** PostgreSQL
* **Test Framework:** xUnit, Microsoft.NET.Test.Sdk

---

## API Specification

### Resources

| Method | Endpoint | Description | Status Codes |
|---|---|---|---|
| `GET` | `/resources` | Retrieve all registered resources | `200 OK` |
| `GET` | `/resources/{id}` | Retrieve details for a specific resource | `200 OK`, `404 Not Found` |
| `POST` | `/resources` | Register a new resource | `201 Created`, `400 Bad Request` |
| `GET` | `/resources/{id}/bookings` | Retrieve all bookings associated with a resource | `200 OK` |

### Bookings

| Method | Endpoint | Description | Status Codes |
|---|---|---|---|
| `GET` | `/bookings` | Retrieve all bookings across resources | `200 OK` |
| `GET` | `/bookings/{id}` | Retrieve booking details by ID | `200 OK`, `404 Not Found` |
| `POST` | `/bookings` | Create a new booking with conflict validation | `201 Created`, `400 Bad Request`, `409 Conflict` |
| `POST` | `/bookings/{id}/cancel` | Cancel an active reservation | `200 OK`, `400 Bad Request`, `404 Not Found` |

---

## Domain Rules and Overlap Detection

A requested booking overlaps with an existing reservation if all of the following conditions hold:
1. Both reservations reference the same resource ID (`ResourceId == other.ResourceId`).
2. Neither reservation is in a cancelled state (`!IsCancelled && !other.IsCancelled`).
3. The time intervals intersect:
   $$\neg (\text{End} \le \text{other.Start} \lor \text{Start} \ge \text{other.End})$$

Adjacent bookings (where `BookingA.End == BookingB.Start`) are permitted, allowing seamless back-to-back utilization.

---

## Automated Tests

Unit tests reside in the `ResourceBooking.Tests` project and can be executed via the .NET CLI:

```bash
dotnet test
```

All unit tests validate boundary conditions, period ordering, and overlap calculation.

---

## Getting Started Locally

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [PostgreSQL](https://www.postgresql.org/) instance running locally or via Docker

### Database Configuration
Configure the connection string in `appsettings.json` (or supply the corresponding environment variable):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=resource_booking_db;Username=postgres;Password=your_password"
  }
}
```

### Apply Migrations
```bash
dotnet ef database update
```

### Run Application
```bash
dotnet run --project ResourceBooking.csproj
```
The API listens on the configured local ports (default: `http://localhost:5000` / `https://localhost:5001`).
