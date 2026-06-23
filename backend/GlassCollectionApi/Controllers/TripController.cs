using GlassCollectionApi.Models;
using GlassCollectionApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GlassCollectionApi.Controllers;

[ApiController]
[Route("api/trip")]
public class TripController : ControllerBase
{
    private readonly TripService _tripService;

    public TripController(TripService tripService)
    {
        _tripService = tripService;
    }

    /// <summary>Today's supplier list with Dijkstra-optimized stop sequence and live statuses.</summary>
    [HttpGet("today")]
    public async Task<IActionResult> GetToday(CancellationToken ct)
    {
        var trip = await _tripService.GetTodayTripAsync(ct);

        return Ok(new
        {
            trip.Id,
            trip.Date,
            start = new { trip.StartLatitude, trip.StartLongitude },
            totalDistanceKm = trip.TotalDistanceKm,
            trip.StartedAt,
            remainingStops = trip.Stops.Count(s => s.Status != StopStatus.Collected),
            stops = trip.Stops
                .OrderBy(s => s.SequenceOrder)
                .Select(s => new
                {
                    s.SequenceOrder,
                    s.SupplierId,
                    supplierName = s.Supplier.Name,
                    s.Supplier.Address,
                    status = s.Status.ToString(),
                    s.Supplier.Latitude,
                    s.Supplier.Longitude,
                    expected = new
                    {
                        clearKg = s.Supplier.ExpectedClearKg,
                        colouredKg = s.Supplier.ExpectedColouredKg
                    }
                })
        });
    }

    /// <summary>Trip report summary for Screen 3 — totals, duration, and shortfall warnings.</summary>
    [HttpGet("today/summary")]
    public async Task<IActionResult> GetTodaySummary(CancellationToken ct)
    {
        var trip = await _tripService.GetTodayTripAsync(ct);
        var completedAt = trip.CompletedAt ?? DateTime.UtcNow;
        var durationMinutes = Math.Round((completedAt - trip.StartedAt).TotalMinutes, 1);

        var stops = trip.Stops
            .OrderBy(s => s.SequenceOrder)
            .Select(s =>
            {
                var collectedClear = s.CollectedClearKg ?? 0;
                var collectedColoured = s.CollectedColouredKg ?? 0;
                var expectedClear = s.Supplier.ExpectedClearKg;
                var expectedColoured = s.Supplier.ExpectedColouredKg;
                var clearShortfall = collectedClear < expectedClear;
                var colouredShortfall = collectedColoured < expectedColoured;

                return new
                {
                    s.SequenceOrder,
                    s.SupplierId,
                    supplierName = s.Supplier.Name,
                    status = s.Status.ToString(),
                    expected = new { clearKg = expectedClear, colouredKg = expectedColoured },
                    collected = new { clearKg = collectedClear, colouredKg = collectedColoured },
                    s.Condition,
                    s.CollectedAt,
                    hasShortfall = clearShortfall || colouredShortfall,
                    warnings = BuildShortfallWarnings(clearShortfall, colouredShortfall, expectedClear, expectedColoured, collectedClear, collectedColoured)
                };
            })
            .ToList();

        return Ok(new
        {
            trip.Id,
            trip.Date,
            totalDistanceKm = trip.TotalDistanceKm,
            trip.StartedAt,
            completedAt = trip.CompletedAt,
            durationMinutes,
            totals = new
            {
                clearKg = stops.Sum(s => s.collected.clearKg),
                colouredKg = stops.Sum(s => s.collected.colouredKg),
                combinedKg = stops.Sum(s => s.collected.clearKg + s.collected.colouredKg)
            },
            shortfallCount = stops.Count(s => s.hasShortfall),
            stops
        });
    }

    private static List<string> BuildShortfallWarnings(
        bool clearShortfall,
        bool colouredShortfall,
        decimal expectedClear,
        decimal expectedColoured,
        decimal collectedClear,
        decimal collectedColoured)
    {
        var warnings = new List<string>();
        if (clearShortfall)
            warnings.Add($"Clear glass shortfall: collected {collectedClear} kg, expected {expectedClear} kg.");
        if (colouredShortfall)
            warnings.Add($"Coloured glass shortfall: collected {collectedColoured} kg, expected {expectedColoured} kg.");
        return warnings;
    }
}
