using Microsoft.EntityFrameworkCore;
using FastPin.Models;
using System.IO;

namespace FastPin.Data
{
    /// <summary>
    /// Database context for FastPin application using SQLite or MySQL
    /// </summary>
    public class FastPinDbContext : DbContext
    {
        private readonly AppSettings _settings;

        public DbSet<PinnedItem> PinnedItems { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<ItemTag> ItemTags { get; set; } = null!;

        public FastPinDbContext()
        {
            _settings = AppSettings.Load();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (_settings.DatabaseType == "MySQL")
                {
                    ConfigureMySql(optionsBuilder);
                }
                else
                {
                    ConfigureSqlite(optionsBuilder);
                }
            }
        }

        private void ConfigureSqlite(DbContextOptionsBuilder optionsBuilder)
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

        private void ConfigureMySql(DbContextOptionsBuilder optionsBuilder)
        {
            var server = _settings.MySqlServer ?? "localhost";
            var port = _settings.MySqlPort ?? 3306;
            var database = _settings.MySqlDatabase ?? "fastpin";
            var username = _settings.MySqlUsername ?? "root";
            var password = _settings.MySqlPassword ?? "";

            var connectionString = $"Server={server};Port={port};Database={database};Uid={username};Pwd={password};";
            
            // Auto-detect MySQL server version - defaults to 8.0 if detection fails
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
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
                entity.Property(e => e.Class).HasMaxLength(50);
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
