using GlassCollectionApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GlassCollectionApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripStop> TripStops => Set<TripStop>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).HasMaxLength(20);
            entity.Property(s => s.Name).HasMaxLength(200).IsRequired();
            entity.Property(s => s.Address).HasMaxLength(500);
            entity.Property(s => s.ExpectedClearKg).HasPrecision(10, 2);
            entity.Property(s => s.ExpectedColouredKg).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.TotalDistanceKm).HasPrecision(10, 2);
            entity.HasIndex(t => t.Date);
        });

        modelBuilder.Entity<TripStop>(entity =>
        {
            entity.HasKey(ts => ts.Id);
            entity.Property(ts => ts.SupplierId).HasMaxLength(20).IsRequired();
            entity.Property(ts => ts.Condition).HasMaxLength(100);
            entity.Property(ts => ts.CollectedClearKg).HasPrecision(10, 2);
            entity.Property(ts => ts.CollectedColouredKg).HasPrecision(10, 2);
            entity.Property(ts => ts.Status).HasConversion<int>();

            entity.HasIndex(ts => new { ts.TripId, ts.SupplierId }).IsUnique();
            entity.HasIndex(ts => new { ts.TripId, ts.SequenceOrder }).IsUnique();

            entity.HasOne(ts => ts.Trip)
                .WithMany(t => t.Stops)
                .HasForeignKey(ts => ts.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ts => ts.Supplier)
                .WithMany(s => s.TripStops)
                .HasForeignKey(ts => ts.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
