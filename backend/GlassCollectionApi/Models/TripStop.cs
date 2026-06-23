namespace GlassCollectionApi.Models;

public class TripStop
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public string SupplierId { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    public StopStatus Status { get; set; } = StopStatus.Pending;
    public decimal? CollectedClearKg { get; set; }
    public decimal? CollectedColouredKg { get; set; }
    public string? Condition { get; set; }
    public DateTime? CollectedAt { get; set; }

    public Trip Trip { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
}
