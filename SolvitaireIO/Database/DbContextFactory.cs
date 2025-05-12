namespace SolvitaireIO.Database;

public class DbContextFactory
{
    public bool InMemory { get; init; }
    public string? OutputDirectory { get; init; }
    public string InMemoryIdentifier { get; init; }

    public DbContextFactory(string? outputDirectory)
    {
        InMemory = outputDirectory == null;
        OutputDirectory = outputDirectory;
        InMemoryIdentifier = Guid.NewGuid().ToString();
    }

    public SolvitaireDbContext CreateDbContext()
    {
        if (InMemory)
        {
            return SolvitaireDbContext.CreateInMemoryDbContext(InMemoryIdentifier);
        }
        else
        {
            return new SolvitaireDbContext(OutputDirectory);
        }
    }
}