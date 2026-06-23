using System.ComponentModel.DataAnnotations;

namespace GlassCollectionApi.DTOs;

public class CollectionSubmissionDto
{
    [Required]
    public string SupplierId { get; set; } = string.Empty;

    [Range(0, 100000)]
    public decimal ClearKg { get; set; }

    [Range(0, 100000)]
    public decimal ColouredKg { get; set; }

    public string Condition { get; set; } = "Good";

    public DateTime? CollectedAt { get; set; }
}

public class SyncCollectionItemDto
{
    [Required]
    public string SupplierId { get; set; } = string.Empty;

    [Range(0, 100000)]
    public decimal ClearKg { get; set; }

    [Range(0, 100000)]
    public decimal ColouredKg { get; set; }

    public string Condition { get; set; } = "Good";

    public DateTime CollectedAt { get; set; }
}

public class SyncCollectionsRequestDto
{
    [Required]
    [MinLength(1)]
    public List<SyncCollectionItemDto> Collections { get; set; } = new();
}
