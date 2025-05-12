using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SolvitaireCore;
using SolvitaireIO.Database.Models;

namespace SolvitaireIO.Database.Repositories;

public class GenerationLogRepository
{
    private readonly SolvitaireDbContext _context;

    public GenerationLogRepository(SolvitaireDbContext context)
    {
        _context = context;
    }

    public async Task AddGenerationAsync(GenerationLog log)
    {
        // Ensure the BestChromosome is not added again
        var bestChromosomeEntry = _context.Entry(log.BestChromosome);
        if (bestChromosomeEntry.State == EntityState.Detached)
        {
            // Attach the chromosome to the DbContext if it's not already tracked
            _context.Chromosomes.Attach(log.BestChromosome);
        }
        

        var existingEntity = await _context.Generations.FindAsync(log.Generation);
        if (existingEntity != null)
        {
            // Update the existing entity if needed
            _context.Entry(existingEntity).CurrentValues.SetValues(log);
        }
        else
        {
            // Add the new entity
            _context.Generations.Add(log);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<GenerationLog>> GetAllGenerationLogsAsync()
    {
        var toreturn =  await _context.Generations
            .Include(g => g.BestChromosome)
            .Include(g => g.AverageChromosome)
            .Include(g => g.StdChromosome)
            .ToListAsync();
        return toreturn;
    }

    public async Task<GenerationLog?> GetLastGenerationAsync()
    {
        return await _context.Generations
            .Include(g => g.BestChromosome)
            .Include(g => g.AverageChromosome)
            .Include(g => g.StdChromosome)
            .OrderByDescending(g => g.Generation)
            .FirstOrDefaultAsync();
    }

    public async Task<GenerationLog?> GetGenerationLogWithAgentsAsync(int generation)
    {
        return await _context.Generations
            .Include(g => g.AgentLogs) // Eagerly load AgentRepository
            .FirstOrDefaultAsync(g => g.Generation == generation);
    }

    public async Task<GenerationLog?> GetGenerationLogWithChromosomesAsync(int generation)
    {
        return await _context.Generations
            .Include(g => g.BestChromosome)
            .Include(g => g.AverageChromosome)
            .Include(g => g.StdChromosome)
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

    public async Task AddAgentAsync(AgentLog log)
    {
        var existingChromosome = await _context.Chromosomes
            .FindAsync(log.Chromosome.StableHash);

        if (existingChromosome != null)
        {
            _context.Entry(existingChromosome).State = EntityState.Unchanged;
            log.Chromosome = existingChromosome;
            if (!existingChromosome.Chromosome.Equals(log.Chromosome.Chromosome))
                Debugger.Break();
        }

        if (log.Chromosome == null)
            Debugger.Break();

        _context.Agents.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AgentLog>> GetAgentLogsByGenerationAsync(int generation = -1)
    {
        if (generation == -1)
            return await _context.Agents
                .Include(p => p.Chromosome)
                .ToListAsync();
        return await _context.Agents
            .Where(log => log.Generation == generation)
            .Include(p => p.Chromosome)
            .ToListAsync();
    }

    public async Task<List<TChromosome>> GetChromosomesByGenerationAsync<TChromosome>(int generation)
       where TChromosome : Chromosome, new()
    {
        var chromosomeLogs = await _context.Chromosomes
            .Include(c => c.AgentLog)
            .Where(c => c.AgentLog != null && c.AgentLog.Generation == generation)
            .ToListAsync();

        if (chromosomeLogs == null || !chromosomeLogs.Any())
            return new List<TChromosome>();

        var result = new List<TChromosome>();

        foreach (var chromosomeLog in chromosomeLogs)
        {
            if (chromosomeLog.AgentLog != null)
            {
                for (int i = 0; i < chromosomeLog.AgentLog.Count; i++)
                {
                    result.Add(DeserializeChromosome<TChromosome>(chromosomeLog));
                }
            }
        }

        return result;
    }

    private TChromosome DeserializeChromosome<TChromosome>(ChromosomeLog chromosomeLog)
        where TChromosome : Chromosome, new()
    {
        // Deserialize the gene data into a TChromosome instance
        var chromosome = new TChromosome();
        chromosome.LoadGeneData(chromosomeLog.GeneData);
        return chromosome;
    }
}

public class ChromosomeRepository
{
    private readonly SolvitaireDbContext _context;
    public ChromosomeRepository(SolvitaireDbContext context)
    {
        _context = context;
    }

    public async Task AddChromosomeLogAsync(ChromosomeLog chromosomeLog)
    {
        _context.Chromosomes.Add(chromosomeLog);
        await _context.SaveChangesAsync();
    }

    public async Task AddChromosomeAsync<TChromosome>(TChromosome chromosome)
        where TChromosome : Chromosome
    {
        var chromosomeLog = ChromosomeLog.FromChromosome(chromosome);

        _context.Chromosomes.Add(chromosomeLog);
        await _context.SaveChangesAsync();
    }
}
