using Microsoft.EntityFrameworkCore;
using SolvitaireIO.Database.Models;

namespace SolvitaireIO.Database.Repositories;

public class GenerationLogRepository
{
    private readonly SolvitaireDbContext _context;

    public GenerationLogRepository(SolvitaireDbContext context)
    {
        _context = context;
    }

    public async Task AddGenerationLogAsync(GenerationLog log)
    {
        _context.Generations.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<GenerationLog>> GetAllGenerationLogsAsync()
    {
        return await _context.Generations.ToListAsync();
    }

    public async Task<GenerationLog?> GetGenerationLogWithAgentsAsync(int generation)
    {
        return await _context.Generations
            .Include(g => g.AgentLogs) // Eagerly load AgentLogs
            .FirstOrDefaultAsync(g => g.Generation == generation);
    }
}

public class AgentLogRepository
{
    private readonly SolvitaireDbContext _context;

    public AgentLogRepository(SolvitaireDbContext context)
    {
        _context = context;
    }

    public async Task AddAgentLogAsync(AgentLog log)
    {
        _context.Agents.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AgentLog>> GetAgentLogsByGenerationAsync(int generation)
    {
        return await _context.Agents
            .Where(log => log.Generation == generation)
            .ToListAsync();
    }
}
