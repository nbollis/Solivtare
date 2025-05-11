using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SolvitaireCore;
using SolvitaireIO.Database.Models;

namespace SolvitaireIO.Database;

/// <summary>
/// Manages database connections and operations. 
/// </summary>
public class SolvitaireDbContext : DbContext
{
    public DbSet<GenerationLog> GenerationLogs { get; set; }
    public DbSet<Deck> Decks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=solvitaire.db");
    }

    public static SolvitaireDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<SolvitaireDbContext>()
            .UseMemoryCache(new MemoryCache(new MemoryCacheOptions()));

        var context = new SolvitaireDbContext();
        context.OnConfiguring(options);
        return context;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entity relationships and constraints here
    }
}
