using SolvitaireIO.Database.Repositories;

namespace SolvitaireIO.Database;

public interface IRepositoryManager : IDisposable
{
    GenerationLogRepository GenerationRepository { get; }
    AgentLogRepository AgentRepository { get; }
    ChromosomeRepository ChromosomeRepository { get; }
    public GenerationLogRepository CreateGenerationRepository();
    public AgentLogRepository CreateAgentRepository();
    public ChromosomeRepository CreateChromosomeRepository();

    Task<int> SaveChangesAsync();
}

public class RepositoryManager : IRepositoryManager
{
    private readonly SolvitaireDbContext _context;
    private readonly DbContextFactory _dbContextFactory;
    public RepositoryManager(DbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _context = _dbContextFactory.CreateDbContext();

        GenerationRepository = new GenerationLogRepository(_context);
        AgentRepository = new AgentLogRepository(_context);
        ChromosomeRepository = new ChromosomeRepository(_context);
    }

    public GenerationLogRepository GenerationRepository { get; }
    public AgentLogRepository AgentRepository { get; }
    public ChromosomeRepository ChromosomeRepository { get; }

    public GenerationLogRepository CreateGenerationRepository()
    {
        return new GenerationLogRepository(_dbContextFactory.CreateDbContext());
    }

    public AgentLogRepository CreateAgentRepository()
    {
        return new AgentLogRepository(_dbContextFactory.CreateDbContext());
    }

    public ChromosomeRepository CreateChromosomeRepository()
    {
        return new ChromosomeRepository(_dbContextFactory.CreateDbContext());
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}