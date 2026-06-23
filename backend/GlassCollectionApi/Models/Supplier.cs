namespace GlassCollectionApi.Models;

public class Supplier
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal ExpectedClearKg { get; set; }
    public decimal ExpectedColouredKg { get; set; }

    public ICollection<TripStop> TripStops { get; set; } = new List<TripStop>();
}
