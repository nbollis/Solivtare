using SolvitaireIO.Database.Repositories;

namespace SolvitaireIO.Database;

public interface IRepositoryManager : IDisposable
{
    GenerationLogRepository GenerationRepository { get; }
    AgentLogRepository AgentRepository { get; }
    ChromosomeRepository ChromosomeRepository { get; }
    Task<int> SaveChangesAsync();
}

public class RepositoryManager : IRepositoryManager
{
    private readonly SolvitaireDbContext _context;

    public RepositoryManager(SolvitaireDbContext context)
    {
        _context = context;
        GenerationRepository = new GenerationLogRepository(_context);
        AgentRepository = new AgentLogRepository(_context);
        ChromosomeRepository = new ChromosomeRepository(_context);
    }

    public GenerationLogRepository GenerationRepository { get; }
    public AgentLogRepository AgentRepository { get; }
    public ChromosomeRepository ChromosomeRepository { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}