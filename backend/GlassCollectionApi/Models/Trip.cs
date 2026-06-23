namespace GlassCollectionApi.Models;

public class Trip
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public decimal TotalDistanceKm { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<TripStop> Stops { get; set; } = new List<TripStop>();
}
