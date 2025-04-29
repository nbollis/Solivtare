using System.Collections.Concurrent;

namespace SolvitaireGenetics;

public abstract class GeneticAlgorithm<TChromosome> where TChromosome : Chromosome<TChromosome>
{
    protected readonly GeneticAlgorithmLogger<TChromosome> Logger;
    protected readonly int PopulationSize;
    protected readonly Random Random = new();
    protected readonly double MutationRate;
    protected readonly int TournamentSize;

    protected int CurrentGeneration { get; private set; }

    // Fitness cache
    private readonly Dictionary<int, double> _fitnessCache = new();

    protected GeneticAlgorithm(int populationSize, double mutationRate, int tournamentSize, string outputDirectory)
    {
        PopulationSize = populationSize;
        MutationRate = mutationRate;
        TournamentSize = tournamentSize;
        Logger = new GeneticAlgorithmLogger<TChromosome>(outputDirectory);
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

    public TChromosome RunEvolution(int generations)
    {
        List<TChromosome> population = InitializePopulation();

        for (int generation = 0; generation < generations; generation++)
        {
            CurrentGeneration = generation;
            Console.WriteLine($"Generation {generation}: Evaluating population...");

            population = EvolvePopulation(population, out List<double> fitness); 

            double bestFitness = fitness[0];
            TChromosome bestChromosome = population[0];
            TChromosome averageChromosome = Chromosome<TChromosome>.GetAverageChromosome(population);
            TChromosome stdChromosome = Chromosome<TChromosome>.GetStandardDeviationChromosome(population);

            Logger.LogGenerationInfo(generation, bestFitness, fitness.Average(), bestChromosome, averageChromosome, stdChromosome);

        }

        return population[0]; // Best chromosome
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

            var child = Chromosome<TChromosome>.Crossover(parent1, parent2);
            child = Chromosome<TChromosome>.Mutate(child, MutationRate);

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
            .Select(_ => Chromosome<TChromosome>.CreateRandom(Random))
            .ToList();
    }

    protected virtual void FlushLogs()
    {
        Logger.FlushAgentLogs();
    }
}