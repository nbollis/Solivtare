using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace SolvitaireGenetics;

public abstract class GeneticAlgorithm<TChromosome> where TChromosome : Chromosome<TChromosome>
{
    protected readonly ILogger<GeneticAlgorithm<TChromosome>> Logger;
    protected readonly int PopulationSize;
    protected readonly Random Random = new();
    protected readonly double MutationRate;
    protected readonly int TournamentSize;

    // Fitness cache
    private readonly Dictionary<int, double> _fitnessCache = new();

    public GeneticAlgorithm(int populationSize, double mutationRate, int tournamentSize, ILogger<GeneticAlgorithm<TChromosome>> logger)
    {
        PopulationSize = populationSize;
        MutationRate = mutationRate;
        TournamentSize = tournamentSize;
        Logger = logger;
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
            Logger.LogInformation($"Generation {generation}: Evaluating population...");

            population = EvolvePopulation(population, out List<double> fitness); 

            double bestFitness = fitness[0];
            TChromosome bestChromosome = population[0];
            TChromosome averageChromosome = Chromosome<TChromosome>.GetAverageChromosome(population);

            Logger.LogInformation($"Generation {generation}: Best fitness = {bestFitness}, Average fitness = {fitness.Average()}");
            Logger.LogInformation($"BestChromosome = {bestChromosome.SerializeWeights()}, AverageChromosome = {averageChromosome.SerializeWeights()}");
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
        Parallel.ForEach(newPopulation, chromosome =>
        {
            double fitnessValue = GetFitness(chromosome);
            fitnessResults[chromosome] = fitnessValue;
        });

        // Sort the new population by fitness (descending)
        newPopulation = fitnessResults
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        // Cache fitness values for the new population
        fitness.AddRange(fitnessResults.Values);

        return newPopulation;
    }

    /// <summary>
    /// Grab a random sample of size _tournamentSize from the population and return the best one.
    /// </summary>
    protected TChromosome TournamentSelection(List<TChromosome> population)
    {
        var tournament = new ConcurrentBag<TChromosome>();
        var fitnessResults = new ConcurrentDictionary<TChromosome, double>();

        // Select random chromosomes for the tournament in parallel  
        for (int i = 0; i < TournamentSize; i++)
        {
            tournament.Add(population[Random.Next(population.Count)]);
        }

        // Calculate fitness for each chromosome in the tournament in parallel  
        Parallel.ForEach(tournament, chromosome =>
        {
            double fitnessValue = GetFitness(chromosome);
            fitnessResults[chromosome] = fitnessValue;
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
}