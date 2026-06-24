using FastPin.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FastPin.Api.Data;

public class FastPinApiDbContext : DbContext
{
    public FastPinApiDbContext(DbContextOptions<FastPinApiDbContext> options) : base(options)
    {
    }

    public DbSet<PinnedItem> PinnedItems { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<ItemTag> ItemTags { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PinnedItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.CreatedDate).IsRequired();
            entity.Property(e => e.ModifiedDate).IsRequired();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Class).HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<ItemTag>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.PinnedItem)
                .WithMany(p => p.ItemTags)
                .HasForeignKey(e => e.PinnedItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Tag)
                .WithMany(t => t.ItemTags)
                .HasForeignKey(e => e.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.PinnedItemId, e.TagId }).IsUnique();
        });
    }
}
