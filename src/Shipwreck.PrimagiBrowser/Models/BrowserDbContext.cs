using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Shipwreck.PrimagiBrowser.Models;

public sealed class BrowserDbContext : DbContext
{
    private static readonly string _DataPath
        = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), typeof(App).Namespace!, "data.db");

    public DbSet<CharacterRecord>? Characters { get; set; }
    public DbSet<CoordinateRecord>? Coordinates { get; set; }
    public DbSet<PhotoRecord>? Photo { get; set; }

    private static Task? _InitializeDbContextTask;

    public static async Task<BrowserDbContext> CreateDbAsync()
    {
        if (_InitializeDbContextTask == null)
        {
            static async Task initializeTask()
            {
                var f = new FileInfo(_DataPath);
                if (!f.Directory!.Exists)
                {
                    f.Directory.Create();
                }

                using var db = new BrowserDbContext();
                if (!f.Exists)
                {
                    await db.Database.EnsureCreatedAsync().ConfigureAwait(false);
                }
                await db.Database.MigrateAsync().ConfigureAwait(false);
            }
            _InitializeDbContextTask = initializeTask();
        }
        await _InitializeDbContextTask.ConfigureAwait(false);

        return new BrowserDbContext();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={_DataPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CoordinateRecord>().HasKey(nameof(CoordinateRecord.CharacterId), nameof(CoordinateRecord.SealId));
        modelBuilder.Entity<PhotoRecord>().HasKey(nameof(PhotoRecord.CharacterId), nameof(PhotoRecord.Seq));
    }
}