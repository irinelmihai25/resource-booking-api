using ResourceBooking.Models;
using Xunit;

namespace ResourceBooking.Tests;

public class BookingTests
{
    // ==========================================
    // Teste pentru HasValidPeriod()
    // ==========================================

    [Fact]
    public void HasValidPeriod_WhenStartIsBeforeEnd_ReturnsTrue()
    {
        // Arrange
        var booking = new Booking
        {
            Start = new DateTimeOffset(2026, 10, 18, 10, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 10, 18, 11, 0, 0, TimeSpan.Zero)
        };

        // Act
        var result = booking.HasValidPeriod();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasValidPeriod_WhenStartIsAfterEnd_ReturnsFalse()
    {
        // Arrange
        var booking = new Booking
        {
            Start = new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 10, 18, 11, 0, 0, TimeSpan.Zero)
        };

        // Act
        var result = booking.HasValidPeriod();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasValidPeriod_WhenStartEqualsEnd_ReturnsFalse()
    {
        // Arrange: interval de 0 minute (Start == End) nu este o rezervare valida
        var booking = new Booking
        {
            Start = new DateTimeOffset(2026, 10, 18, 10, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 10, 18, 10, 0, 0, TimeSpan.Zero)
        };

        // Act
        var result = booking.HasValidPeriod();

        // Assert
        Assert.False(result);
    }

    // ==========================================
    // Teste pentru Overlaps(Booking other)
    // ==========================================

    [Fact]
    public void Overlaps_WhenBookingsIntersectForSameResource_ReturnsTrue()
    {
        // Arrange: 10:00-12:00 si 11:00-13:00 pe aceeasi resursa (Id 1)
        var bookingA = new Booking
        {
            ResourceId = 1,
            Start = new DateTimeOffset(2026, 10, 18, 10, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero)
        };

        var bookingB = new Booking
        {
            ResourceId = 1,
            Start = new DateTimeOffset(2026, 10, 18, 11, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 10, 18, 13, 0, 0, TimeSpan.Zero)
        };

        // Act & Assert
        Assert.True(bookingA.Overlaps(bookingB));
        Assert.True(bookingB.Overlaps(bookingA)); // simetrie: A se suprapune cu B si invers
    }

    [Fact]
    public void Overlaps_WhenBookingsAreAdjacent_ReturnsFalse()
    {
        // Arrange: 10:00-11:00 si 11:00-12:00 (se ating la ora 11:00, dar nu se suprapun)
        var bookingA = new Booking
        {
            ResourceId = 1,
            Start = new DateTimeOffset(2026, 10, 18, 10, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 10, 18, 11, 0, 0, TimeSpan.Zero)
        };

        var bookingB = new Booking
        {
            ResourceId = 1,
            Start = new DateTimeOffset(2026, 10, 18, 11, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero)
        };

        // Act & Assert
        Assert.False(bookingA.Overlaps(bookingB));
    }

    [Fact]
    public void Overlaps_WhenDifferentResources_ReturnsFalse()
    {
        // Arrange: acelasi interval orar, dar Resursa 1 vs Resursa 2
        var bookingA = new Booking
        {
            ResourceId = 1,
            Start = new DateTimeOffset(2026, 10, 18, 10, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero)
        };

        var bookingB = new Booking
        {
            ResourceId = 2,
            Start = new DateTimeOffset(2026, 10, 18, 10, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero)
        };

        // Act & Assert
        Assert.False(bookingA.Overlaps(bookingB));
    }

    [Fact]
    public void Overlaps_WhenOneBookingIsCancelled_ReturnsFalse()
    {
        // Arrange: intervale identice pe aceeasi resursa, dar rezervarea existenta este anulata
        var bookingA = new Booking
        {
            ResourceId = 1,
            Start = new DateTimeOffset(2026, 10, 18, 10, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero)
        };

        var bookingB = new Booking
        {
            ResourceId = 1,
            Start = new DateTimeOffset(2026, 10, 18, 10, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero),
            IsCancelled = true
        };

        // Act & Assert: o rezervare anulata nu blocheaza o alta rezervare
        Assert.False(bookingA.Overlaps(bookingB));
    }
}