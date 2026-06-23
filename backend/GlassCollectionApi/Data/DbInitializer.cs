using GlassCollectionApi.Data;
using GlassCollectionApi.Models;
using GlassCollectionApi.Services;
using Microsoft.EntityFrameworkCore;

namespace GlassCollectionApi.Data;

public static class DbInitializer
{
    private const double StartLatitude = 51.5050;
    private const double StartLongitude = -0.1200;

    public static async Task InitializeAsync(AppDbContext db, RouteOptimizerService routeOptimizer)
    {
        await db.Database.MigrateAsync();

        if (await db.Suppliers.AnyAsync())
            return;

        var suppliers = new List<Supplier>
        {
            new()
            {
                Id = "SUP001",
                Name = "Green Valley Cafe",
                Address = "12 Covent Garden, London",
                Latitude = 51.5074,
                Longitude = -0.1278,
                ExpectedClearKg = 50,
                ExpectedColouredKg = 30
            },
            new()
            {
                Id = "SUP002",
                Name = "Riverside Pub",
                Address = "45 Tower Bridge Rd, London",
                Latitude = 51.5155,
                Longitude = -0.0923,
                ExpectedClearKg = 40,
                ExpectedColouredKg = 25
            },
            new()
            {
                Id = "SUP003",
                Name = "Sunny Supermarket",
                Address = "88 South Bank, London",
                Latitude = 51.5033,
                Longitude = -0.1195,
                ExpectedClearKg = 80,
                ExpectedColouredKg = 45
            },
            new()
            {
                Id = "SUP004",
                Name = "Oak Street Bistro",
                Address = "3 Kensington High St, London",
                Latitude = 51.5120,
                Longitude = -0.1440,
                ExpectedClearKg = 35,
                ExpectedColouredKg = 20
            },
            new()
            {
                Id = "SUP005",
                Name = "City Glass Depot",
                Address = "21 Bermondsey St, London",
                Latitude = 51.4980,
                Longitude = -0.1050,
                ExpectedClearKg = 60,
                ExpectedColouredKg = 35
            }
        };

        db.Suppliers.AddRange(suppliers);

        var route = routeOptimizer.OptimizeRoute(
            StartLatitude,
            StartLongitude,
            suppliers.Select(s => new RouteNode(s.Id, s.Latitude, s.Longitude)).ToList());

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var trip = new Trip
        {
            Date = today,
            StartLatitude = StartLatitude,
            StartLongitude = StartLongitude,
            TotalDistanceKm = (decimal)route.TotalDistanceKm,
            StartedAt = DateTime.UtcNow
        };

        for (var i = 0; i < route.SupplierOrder.Count; i++)
        {
            trip.Stops.Add(new TripStop
            {
                SupplierId = route.SupplierOrder[i],
                SequenceOrder = i + 1,
                Status = i == 0 ? StopStatus.Next : StopStatus.Pending
            });
        }

        db.Trips.Add(trip);
        await db.SaveChangesAsync();
    }
}
