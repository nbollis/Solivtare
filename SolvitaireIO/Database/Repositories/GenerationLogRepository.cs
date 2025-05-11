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
        _context.GenerationLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<GenerationLog>> GetAllGenerationLogsAsync()
    {
        return await _context.GenerationLogs.ToListAsync();
    }
}
