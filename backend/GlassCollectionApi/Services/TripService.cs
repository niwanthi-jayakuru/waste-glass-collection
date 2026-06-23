using GlassCollectionApi.Data;
using GlassCollectionApi.DTOs;
using GlassCollectionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GlassCollectionApi.Services;

public class TripService
{
    private readonly AppDbContext _db;
    private readonly RouteOptimizerService _routeOptimizer;

    public TripService(AppDbContext db, RouteOptimizerService routeOptimizer)
    {
        _db = db;
        _routeOptimizer = routeOptimizer;
    }

    public async Task<Trip> GetTodayTripAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var trip = await _db.Trips
            .Include(t => t.Stops)
            .ThenInclude(s => s.Supplier)
            .FirstOrDefaultAsync(t => t.Date == today, ct);

        if (trip is not null)
        {
            await EnsureRouteOptimizedAsync(trip, ct);
            return trip;
        }

        return await CreateTodayTripAsync(ct);
    }

    public async Task<TripStop> SubmitCollectionAsync(CollectionSubmissionDto dto, CancellationToken ct = default)
    {
        var trip = await GetTodayTripAsync(ct);
        var nextStop = trip.Stops
            .OrderBy(s => s.SequenceOrder)
            .FirstOrDefault(s => s.Status == StopStatus.Next);

        if (nextStop is null)
            throw new InvalidOperationException("No remaining stops on today's trip.");

        if (!string.Equals(nextStop.SupplierId, dto.SupplierId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Supplier '{dto.SupplierId}' does not match the expected next stop '{nextStop.SupplierId}'.");

        nextStop.CollectedClearKg = dto.ClearKg;
        nextStop.CollectedColouredKg = dto.ColouredKg;
        nextStop.Condition = dto.Condition;
        nextStop.CollectedAt = dto.CollectedAt ?? DateTime.UtcNow;
        nextStop.Status = StopStatus.Collected;

        var following = trip.Stops
            .Where(s => s.Status == StopStatus.Pending)
            .OrderBy(s => s.SequenceOrder)
            .FirstOrDefault();

        if (following is not null)
        {
            following.Status = StopStatus.Next;
        }
        else
        {
            trip.CompletedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return nextStop;
    }

    public async Task<SyncResult> SyncCollectionsAsync(IReadOnlyList<SyncCollectionItemDto> items, CancellationToken ct = default)
    {
        var trip = await GetTodayTripAsync(ct);
        var synced = 0;
        var errors = new List<string>();

        foreach (var item in items)
        {
            var stop = trip.Stops.FirstOrDefault(s =>
                string.Equals(s.SupplierId, item.SupplierId, StringComparison.OrdinalIgnoreCase));

            if (stop is null)
            {
                errors.Add($"Unknown supplier '{item.SupplierId}'.");
                continue;
            }

            stop.CollectedClearKg = item.ClearKg;
            stop.CollectedColouredKg = item.ColouredKg;
            stop.Condition = item.Condition;
            stop.CollectedAt = item.CollectedAt;
            stop.Status = StopStatus.Collected;
            synced++;
        }

        if (trip.Stops.All(s => s.Status == StopStatus.Collected))
        {
            trip.CompletedAt ??= DateTime.UtcNow;
        }
        else
        {
            foreach (var stop in trip.Stops)
            {
                if (stop.Status != StopStatus.Collected)
                    stop.Status = StopStatus.Pending;
            }

            var next = trip.Stops
                .Where(s => s.Status == StopStatus.Pending)
                .OrderBy(s => s.SequenceOrder)
                .First();
            next.Status = StopStatus.Next;
        }

        await _db.SaveChangesAsync(ct);
        return new SyncResult(synced, items.Count - synced, errors);
    }

    private async Task<Trip> CreateTodayTripAsync(CancellationToken ct)
    {
        var suppliers = await _db.Suppliers.OrderBy(s => s.Id).ToListAsync(ct);
        const double startLat = 51.5050;
        const double startLon = -0.1200;

        var route = _routeOptimizer.OptimizeRoute(
            startLat,
            startLon,
            suppliers.Select(s => new RouteNode(s.Id, s.Latitude, s.Longitude)).ToList());

        var trip = new Trip
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            StartLatitude = startLat,
            StartLongitude = startLon,
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

        _db.Trips.Add(trip);
        await _db.SaveChangesAsync(ct);

        await _db.Entry(trip).Collection(t => t.Stops).Query()
            .Include(s => s.Supplier)
            .LoadAsync(ct);

        return trip;
    }

    private async Task EnsureRouteOptimizedAsync(Trip trip, CancellationToken ct)
    {
        if (trip.TotalDistanceKm > 0)
            return;

        var anyCollected = trip.Stops.Any(s => s.Status == StopStatus.Collected);
        if (anyCollected)
            return;

        var suppliers = trip.Stops.Select(s => s.Supplier).Distinct().ToList();
        var route = _routeOptimizer.OptimizeRoute(
            trip.StartLatitude,
            trip.StartLongitude,
            suppliers.Select(s => new RouteNode(s.Id, s.Latitude, s.Longitude)).ToList());

        trip.TotalDistanceKm = (decimal)route.TotalDistanceKm;

        foreach (var stop in trip.Stops)
            stop.Status = StopStatus.Pending;

        for (var i = 0; i < route.SupplierOrder.Count; i++)
        {
            var stop = trip.Stops.First(s => s.SupplierId == route.SupplierOrder[i]);
            stop.SequenceOrder = i + 1;
            stop.Status = i == 0 ? StopStatus.Next : StopStatus.Pending;
        }

        await _db.SaveChangesAsync(ct);
    }
}

public record SyncResult(int Synced, int Skipped, IReadOnlyList<string> Errors);
