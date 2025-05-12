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
    /// <summary>
    /// Manages database connections and operations. 
    /// </summary>
    public SolvitaireDbContext(DbContextOptions<SolvitaireDbContext> options) : base(options)
    {
    }

    public SolvitaireDbContext(string outputDirectory) : base(new DbContextOptionsBuilder<SolvitaireDbContext>()
        .UseSqlite($"Data Source={outputDirectory}/solvitaire.db").Options)
    {
    }

    public DbSet<AgentLog> Agents { get; set; }
    public DbSet<GenerationLog> Generations { get; set; }
    public DbSet<ChromosomeLog> Chromosomes { get; set; } // TODO: avoid duplication
    //public DbSet<Deck> Decks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=solvitaire.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

    }

    public static SolvitaireDbContext CreateInMemoryDbContext(string? databaseName = null)
    {
        databaseName ??= Guid.NewGuid().ToString(); // Generate a unique name if none is provided

        var options = new DbContextOptionsBuilder<SolvitaireDbContext>()
            .UseInMemoryDatabase(databaseName) // Use the unique database name
            .Options;

        var context =  new SolvitaireDbContext(options);
        return context;
    }
}