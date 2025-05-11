using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SolvitaireCore;
using SolvitaireIO.Database.Models;

namespace SolvitaireIO.Database;

/// <summary>
/// Manages database connections and operations. 
/// </summary>
public class SolvitaireDbContext(DbContextOptions<SolvitaireDbContext> options) : DbContext(options)
{
    public DbSet<AgentLog> Agents { get; set; }
    public DbSet<GenerationLog> Generations { get; set; }
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
        // Configure the GenerationLog table
        modelBuilder.Entity<GenerationLog>(entity =>
        {
            entity.ToTable("generations");
            entity.HasKey(e => e.Generation);
        });

        // Configure the AgentLog table
        modelBuilder.Entity<AgentLog>(entity =>
        {
            entity.ToTable("agents");
            entity.HasKey(e => e.Id);
            entity.HasOne<GenerationLog>()
                .WithMany()
                .HasForeignKey(a => a.Generation)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public static SolvitaireDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<SolvitaireDbContext>()
            .UseMemoryCache(new MemoryCache(new MemoryCacheOptions()));

        return new SolvitaireDbContext(options.Options);
    }
}
