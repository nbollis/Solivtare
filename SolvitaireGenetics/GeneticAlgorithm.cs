using System.Collections.Concurrent;
using MathNet.Numerics.Statistics;

namespace SolvitaireGenetics;

// stupid interface here only so that i can do the gui part without 8 million different controls and converters. 
public interface IGeneticAlgorithm
{
    public event Action<int, GenerationLogDto>? GenerationCompleted;
    public void RunEvolution(int generations);
    public void WriteParameters();
}

public abstract class GeneticAlgorithm<TChromosome, TParameters> : IGeneticAlgorithm
    where TChromosome : Chromosome 
    where TParameters : GeneticAlgorithmParameters 

{
    public event Action<int, GenerationLogDto>? GenerationCompleted;
    protected readonly TParameters Parameters;
    protected readonly GeneticAlgorithmLogger<TChromosome> Logger;
    protected readonly int PopulationSize;
    protected readonly Random Random = new();
    protected readonly double MutationRate;
    protected readonly int TournamentSize;

    protected int CurrentGeneration { get;  set; }

    // Fitness cache
    private readonly Dictionary<int, double> _fitnessCache = new();

    protected GeneticAlgorithm(TParameters parameters)
    {
        CurrentGeneration = 0;
        Parameters = parameters;
        PopulationSize = parameters.PopulationSize;
        MutationRate = parameters.MutationRate;
        TournamentSize = parameters.TournamentSize;
        Logger = new GeneticAlgorithmLogger<TChromosome>(parameters.OutputDirectory!);
        Logger.SubscribeToAlgorithm(this);
    }

    protected abstract double EvaluateFitness(TChromosome chromosome);
    private double GetFitness(TChromosome chromosome)
    {
        // Serialize the chromosome to use as a cache key
        int chromosomeKey = chromosome.GetHashCode();

        // Check if the fitness is already cached
        if (_fitnessCache.TryGetValue(chromosomeKey, out double cachedFitness))
        {
            return cachedFitness;
        }

        // If not cached, evaluate and store the fitness
        double fitness = EvaluateFitness(chromosome);
        _fitnessCache[chromosomeKey] = fitness;
        return fitness;
    }

    public void RunEvolution(int generations)
    {
        List<TChromosome> population = InitializePopulation();

        Console.WriteLine($"Starting Evolution with {population.Count} chromosomes.");
        Console.WriteLine($"Population Size: {PopulationSize}");
        Console.WriteLine($"Mutation Rate: {MutationRate}");
        Console.WriteLine($"Tournament Size: {TournamentSize}");
        Console.WriteLine($"Output Directory: {Logger.OutputDirectory}");
        int generation = CurrentGeneration;
        for (; generation < generations; generation++)
        {
            CurrentGeneration = generation;
            Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: Generation {generation}: Evaluating population...");

            population = EvolvePopulation(population, out List<double> fitness); 

            var generationLog = new GenerationLogDto
            {
                Generation = generation,
                BestFitness = fitness[0],
                AverageFitness = fitness.Average(),
                StdFitness = fitness.StandardDeviation(),
                BestChromosome = new ChromosomeDto { Weights = population[0].MutableStatsByName },
                AverageChromosome = new ChromosomeDto { Weights = Chromosome.GetAverageChromosome(population).MutableStatsByName },
                StdChromosome = new ChromosomeDto { Weights = Chromosome.GetStandardDeviationChromosome(population).MutableStatsByName }
            };

            // Fire the GenerationCompleted event
            GenerationCompleted?.Invoke(generation, generationLog);

            FlushLogs();
        }
    }

    /// <summary>
    /// Evolves the population by selecting parents,
    /// performing crossover and mutation to create a new population.
    /// </summary>
    protected List<TChromosome> EvolvePopulation(List<TChromosome> currentPopulation, out List<double> fitness)
    {
        List<TChromosome> newPopulation = [];
        fitness = new List<double>();

        while (newPopulation.Count < PopulationSize)
        {
            var parent1 = TournamentSelection(currentPopulation);
            var parent2 = TournamentSelection(currentPopulation);

            var child = Chromosome.Crossover(parent1, parent2);
            child = Chromosome.Mutate(child, MutationRate);

            newPopulation.Add(child);
        }

        // Evaluate fitness in parallel
        var fitnessResults = new ConcurrentDictionary<TChromosome, double>();
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
                fitnessResults[chromosome] = fitnessValue;
            }
        });

        // Sort the new population by fitness (descending)
        newPopulation = fitnessResults
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        // Cache fitness values for the new population
        fitness.AddRange(fitnessResults.Values.OrderByDescending(kvp => kvp));

        return newPopulation;
    }

    /// <summary>
    /// Grab a random sample of size _tournamentSize from the population and return the best one.
    /// </summary>
    protected TChromosome TournamentSelection(List<TChromosome> population)
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
    protected virtual List<TChromosome> InitializePopulation()
    {
        return Enumerable.Range(0, PopulationSize)
            .Select(_ => Chromosome.CreateRandom<TChromosome>(Random))
            .ToList();
    }

    protected virtual void FlushLogs()
    {
        Logger.FlushAgentLogs();
    }

    public void WriteParameters()
    {
        var configFilePath = Path.Combine(Logger.OutputDirectory, "RunParameters.json");
        if (File.Exists(configFilePath))
        {
            File.Delete(configFilePath);
        }

        Parameters.SaveToFile(configFilePath);
    }
}