using System.Collections.Concurrent;
using MathNet.Numerics.Statistics;

namespace SolvitaireGenetics;

public abstract class GeneticAlgorithm<TChromosome, TParameters> : IGeneticAlgorithm
    where TChromosome : Chromosome 
    where TParameters : GeneticAlgorithmParameters 

{
    public event Action<int, GenerationLogDto>? GenerationCompleted;
    public virtual event Action<AgentLog>? AgentCompleted;
    protected readonly TParameters Parameters;
    protected readonly TChromosome? ChromosomeTemplate;
    protected readonly GeneticAlgorithmLogger<TChromosome>? Logger;
    protected readonly int PopulationSize;
    protected readonly Random Random = new();
    protected readonly double MutationRate;
    protected readonly double CrossOverRate = 0.5;
    protected readonly int TournamentSize;

    public int CurrentGeneration { get; protected set; }

    // Fitness cache
    private readonly ConcurrentDictionary<int, double> _fitnessCache = new();

    protected GeneticAlgorithm(TParameters parameters, TChromosome? chromosomeTemplate = null)
    {
        CurrentGeneration = 0;
        Parameters = parameters;
        PopulationSize = parameters.PopulationSize;
        MutationRate = parameters.MutationRate;
        TournamentSize = parameters.TournamentSize;
        ChromosomeTemplate = chromosomeTemplate;


        if (string.IsNullOrEmpty(parameters.OutputDirectory)) return;
        Logger = new GeneticAlgorithmLogger<TChromosome>(parameters.OutputDirectory!);
        Logger.SubscribeToAlgorithm(this);
    }

    public abstract double EvaluateFitness(TChromosome chromosome);
    private double GetFitness(TChromosome chromosome)
    {
        // Serialize the chromosome to use as a cache key
        int chromosomeKey = chromosome.GetHashCode();

        // Use GetOrAdd to ensure thread-safe access to the cache
        return _fitnessCache.GetOrAdd(chromosomeKey, _ => EvaluateFitness(chromosome));
    }

    public Chromosome RunEvolution(int generations)
    {
        List<TChromosome> population = InitializePopulation();

        int endGeneration = CurrentGeneration + generations;
        for (; CurrentGeneration < endGeneration; CurrentGeneration++)
        {
            Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: Generation {CurrentGeneration}: Evaluating population...");

            // population is ordered by fitness, so the first chromosome is the best one
            population = EvolvePopulation(population);

            var fitness = population.Select(chr => chr.Fitness).ToList();
            var generationLog = new GenerationLogDto
            {
                Generation = CurrentGeneration,
                BestFitness = fitness[0],
                AverageFitness = fitness.Average(),
                StdFitness = fitness.StandardDeviation(),
                BestChromosome = new ChromosomeDto { Weights = population[0].MutableStatsByName },
                AverageChromosome = new ChromosomeDto { Weights = Chromosome.GetAverageChromosome(population).MutableStatsByName },
                StdChromosome = new ChromosomeDto { Weights = Chromosome.GetStandardDeviationChromosome(population).MutableStatsByName }
            };

            // Fire the GenerationCompleted event  
            GenerationCompleted?.Invoke(CurrentGeneration, generationLog);
            Logger?.FlushAgentLogs();
        }

        // Return the best chromosome from the last generation
        var bestChromosome = population[0];
        bestChromosome.Fitness = GetFitness(bestChromosome);
        return bestChromosome;
    }

    /// <summary>
    /// Evolves the population by selecting parents,
    /// performing crossover and mutation to create a new population.
    /// </summary>
    public List<TChromosome> EvolvePopulation(List<TChromosome> currentPopulation)
    {
        List<TChromosome> newPopulation = [];

        while (newPopulation.Count < PopulationSize)
        {
            var parent1 = TournamentSelection(currentPopulation);
            var parent2 = TournamentSelection(currentPopulation);

            var child = Chromosome.Crossover(parent1, parent2);
            child = Chromosome.Mutate(child, MutationRate);

            newPopulation.Add(child);
        }

        // Split the population into ranges for parallel processing
        int rangeSize = (int)Math.Ceiling((double)newPopulation.Count / Environment.ProcessorCount);
        Parallel.For(0, Environment.ProcessorCount, rangeIndex =>
        {
            int start = rangeIndex * rangeSize;
            int end = Math.Min(start + rangeSize, newPopulation.Count);

            for (int i = start; i < end; i++)
            {
                var chromosome = newPopulation[i];
                double fitnessValue = GetFitness(chromosome);
                chromosome.Fitness = fitnessValue;
            }
        });

        //newPopulation.Sort();
        // Sort the new population by fitness (descending)
        newPopulation = newPopulation
            .OrderByDescending(chromosome => chromosome.Fitness)
            .ToList();

        return newPopulation;
    }

    /// <summary>
    /// Grab a random sample of size _tournamentSize from the population and return the best one.
    /// </summary>
    public TChromosome TournamentSelection(List<TChromosome> population)
    {
        var tournamentList = new List<TChromosome>();
        var fitnessResults = new ConcurrentDictionary<TChromosome, double>();

        // Select random chromosomes for the tournament  
        for (int i = 0; i < TournamentSize; i++)
        {
            tournamentList.Add(population[Random.Next(population.Count)]);
        }

        // Calculate fitness for each chromosome in the tournament using range-based parallelization  
        
        int rangeSize = (int)Math.Ceiling((double)tournamentList.Count / Environment.ProcessorCount);
        Parallel.For(0, Environment.ProcessorCount, rangeIndex =>
        {
            int start = rangeIndex * rangeSize;
            int end = Math.Min(start + rangeSize, tournamentList.Count);

            for (int i = start; i < end; i++)
            {
                var chromosome = tournamentList[i];
                double fitnessValue = GetFitness(chromosome);
                fitnessResults[chromosome] = fitnessValue;
            }
        });

        // Return the chromosome with the highest fitness  
        return fitnessResults.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    /// <summary>
    /// Creates a random population of chromosomes with random weights.
    /// </summary>
    /// <returns></returns>
    public List<TChromosome> InitializePopulation()
    {
        // Check if we have a last generation to load
        if (Logger is not null)
        {
            var lastGeneration = Logger.LoadLastGeneration(out int generationNumber);

            if (lastGeneration.Count == PopulationSize && generationNumber == CurrentGeneration - 1)
            {
                // If we have a last generation, use it to initialize the population
                return lastGeneration;
            }
        }

        // If we have a chromosome template, use it to create 10% of the population and mutate them
        // The other 90% will be random chromosomes crossed over with the template. 
        if (ChromosomeTemplate != null)
        {
            int copiesOfBest = PopulationSize / 10;
            var best = Enumerable.Range(0, copiesOfBest).Select(_ => ChromosomeTemplate)
                .Select(b => Chromosome.Mutate(b, MutationRate));

            return Enumerable.Range(0, PopulationSize - copiesOfBest)
                .Select(_ => Chromosome.CreateRandom<TChromosome>(Random)
                        .CrossOver(ChromosomeTemplate) // Temporary? Cross over with best.
                ).Concat(best)
                .ToList();
        }


        // If we don't have a last generation or a template, create a new random population
        return Enumerable.Range(0, PopulationSize)
            .Select(_ => Chromosome.CreateRandom<TChromosome>(Random))
            .ToList();
    }
}

