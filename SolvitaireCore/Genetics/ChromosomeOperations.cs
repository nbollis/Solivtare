namespace SolvitaireCore;

public static class ChromosomeOperations
{
    #region Statistics

    public static TChromosome GetAverageChromosome<TChromosome>(this List<TChromosome> chromosomes)
        where TChromosome : Chromosome
    {
        return Chromosome.GetAverageChromosome(chromosomes);
    }

    public static TChromosome GetStandardDeviationChromosome<TChromosome>(this List<TChromosome> chromosomes)
        where TChromosome : Chromosome
    {
        return Chromosome.GetStandardDeviationChromosome(chromosomes);
    }

    public static TChromosome GetAverageChromosome<TAgent, TChromosome>(this List<TAgent> agents)
        where TChromosome : Chromosome
        where TAgent : IGeneticAgent<TChromosome>
    {
        return Chromosome.GetAverageChromosome(agents.Select(a => a.Chromosome).ToList());
    }

    public static TChromosome GetStandardDeviationChromosome<TAgent, TChromosome>(this List<TAgent> agents)
        where TChromosome : Chromosome
        where TAgent : IGeneticAgent<TChromosome>
    {
        return Chromosome.GetStandardDeviationChromosome(agents.Select(a => a.Chromosome).ToList());
    }

    #endregion
}