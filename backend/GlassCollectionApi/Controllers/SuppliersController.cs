using GlassCollectionApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlassCollectionApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly AppDbContext _db;

    public SuppliersController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Lists all seeded suppliers (for verification during Phase 2).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var suppliers = await _db.Suppliers
            .OrderBy(s => s.Id)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Address,
                s.Latitude,
                s.Longitude,
                s.ExpectedClearKg,
                s.ExpectedColouredKg,
                barcodeNote = $"Generate Code 128 barcode encoding: {s.Id}"
            })
            .ToListAsync();

        return Ok(suppliers);
    }
}
