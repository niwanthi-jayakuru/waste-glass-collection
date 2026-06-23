using GlassCollectionApi.DTOs;
using GlassCollectionApi.Models;
using GlassCollectionApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GlassCollectionApi.Controllers;

[ApiController]
[Route("api/collections")]
public class CollectionsController : ControllerBase
{
    private readonly TripService _tripService;

    public CollectionsController(TripService tripService)
    {
        _tripService = tripService;
    }

    /// <summary>Submit a collection for the current next stop (barcode-verified supplier ID).</summary>
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] CollectionSubmissionDto dto, CancellationToken ct)
    {
        try
        {
            var stop = await _tripService.SubmitCollectionAsync(dto, ct);
            var trip = await _tripService.GetTodayTripAsync(ct);

            var nextStop = trip.Stops
                .OrderBy(s => s.SequenceOrder)
                .FirstOrDefault(s => s.Status == StopStatus.Next);

            return Ok(new
            {
                message = "Collection recorded.",
                supplierId = stop.SupplierId,
                status = stop.Status.ToString(),
                nextStop = nextStop is null ? null : new
                {
                    nextStop.SupplierId,
                    supplierName = nextStop.Supplier.Name,
                    nextStop.SequenceOrder
                },
                tripComplete = trip.CompletedAt is not null
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Final bulk sync of all locally stored collection records from the mobile app.</summary>
    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromBody] SyncCollectionsRequestDto request, CancellationToken ct)
    {
        var result = await _tripService.SyncCollectionsAsync(request.Collections, ct);
        var success = result.Errors.Count == 0;

        return Ok(new
        {
            success,
            synced = result.Synced,
            failed = result.Skipped,
            errors = result.Errors,
            message = success
                ? "All records synced successfully."
                : "Sync completed with some issues."
        });
    }
}
