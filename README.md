# Resource Booking API

A RESTful web API for managing resources (meeting rooms, equipment, workspaces) and scheduling reservations with conflict-prevention business logic, backed by PostgreSQL and Entity Framework Core.

Built with **C#** and **.NET 10** using **ASP.NET Core Minimal APIs**, fully tested with **xUnit**.

---

## 🚀 Features

* **Resource Management:**
  * Create, retrieve, and list bookable resources (e.g. conference rooms, laptops, projectors).
  * Data validation on resource creation.
* **Reservation Lifecycle & Conflict Prevention:**
  * Book resources by specifying start and end timestamps.
  * Automatic normalization to UTC (`DateTimeOffset.ToUniversalTime()`) to prevent timezone issues.
  * Validation rules ensuring start time strictly precedes end time (`HasValidPeriod`).
  * Overlap detection (`Overlaps`) preventing double-booking of any active resource.
  * Booking cancellation endpoint which frees up reserved time slots for new bookings.
  * Guard against duplicate cancellation attempts.
* **Persistence & Database:**
  * Relational database persistence using **PostgreSQL** and **Entity Framework Core**.
  * Code-First workflow with EF Core Migrations.
  * Asynchronous query execution throughout (`ToListAsync`, `FindAsync`, `AnyAsync`, `SaveChangesAsync`).
* **Automated Unit Testing:**
  * Test suite built with **xUnit** covering domain logic.
  * Verifies valid intervals, inverted intervals, zero-duration bookings, intersecting reservations, adjacent intervals (seamless back-to-back bookings), and cancellation behaviors.

---

## 🛠️ Tech Stack

* **Language:** C# 13 / .NET 10
* **Framework:** ASP.NET Core Minimal APIs
* **ORM:** Entity Framework Core 10 (Npgsql.EntityFrameworkCore.PostgreSQL)
* **Database:** PostgreSQL
* **Testing:** xUnit, .NET Test SDK

---

## 📡 API Endpoints

### Resources

| Method | Endpoint | Description | Status Codes |
|---|---|---|---|
| `GET` | `/resources` | List all resources | `200 OK` |
| `GET` | `/resources/{id}` | Get resource details by ID | `200 OK`, `404 Not Found` |
| `POST` | `/resources` | Register a new resource | `201 Created`, `400 Bad Request` |
| `GET` | `/resources/{id}/bookings` | List all bookings for a specific resource | `200 OK` |

### Bookings

| Method | Endpoint | Description | Status Codes |
|---|---|---|---|
| `GET` | `/bookings` | List all bookings across resources | `200 OK` |
| `GET` | `/bookings/{id}` | Get booking by ID | `200 OK`, `404 Not Found` |
| `POST` | `/bookings` | Create a new booking (validates overlap) | `201 Created`, `400 Bad Request`, `409 Conflict` |
| `POST` | `/bookings/{id}/cancel` | Cancel an active booking | `200 OK`, `400 Bad Request`, `404 Not Found` |

---

## 🧠 Business Logic & Domain Rules

### Interval Validation & Overlap Detection
A booking is considered overlapping with an existing reservation if:
1. Both reservations target the **same resource** (`ResourceId == other.ResourceId`).
2. Neither reservation has been **cancelled** (`!IsCancelled && !other.IsCancelled`).
3. The intervals intersect:
   $$\neg (\text{End} \le \text{other.Start} \lor \text{Start} \ge \text{other.End})$$

Adjacent bookings (where `BookingA.End == BookingB.Start`) are explicitly allowed, enabling efficient back-to-back scheduling.

---

## 🧪 Running the Tests

Unit tests are located in `ResourceBooking.Tests` and can be executed via the .NET CLI:

```bash
dotnet test
```

All 7 test cases covering edge cases (strict ordering, adjacent intervals, cancelled slot reuse) will run and report status.

---

## 🏃 Getting Started Locally

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [PostgreSQL](https://www.postgresql.org/) running locally or in Docker

### Configuration
Update the database connection string in `appsettings.json` (or set the environment variable):
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

### Run the Application
```bash
dotnet run --project ResourceBooking.csproj
```
The API will start and listen on configured HTTP/HTTPS ports (default: `http://localhost:5000` / `https://localhost:5001`).
