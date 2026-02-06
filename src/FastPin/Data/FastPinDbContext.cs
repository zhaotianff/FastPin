using Microsoft.EntityFrameworkCore;
using FastPin.Models;
using System.IO;

namespace FastPin.Data
{
    /// <summary>
    /// Database context for FastPin application using SQLite
    /// </summary>
    public class FastPinDbContext : DbContext
    {
        public DbSet<PinnedItem> PinnedItems { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<ItemTag> ItemTags { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Get application data path
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "FastPin"
                );

                // Ensure directory exists
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }

                var dbPath = Path.Combine(appDataPath, "fastpin.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure PinnedItem
            modelBuilder.Entity<PinnedItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();
                entity.Property(e => e.ModifiedDate).IsRequired();
            });

            // Configure Tag
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Configure ItemTag many-to-many relationship
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

                // Ensure unique tag per item
                entity.HasIndex(e => new { e.PinnedItemId, e.TagId }).IsUnique();
            });
        }
    }
}
