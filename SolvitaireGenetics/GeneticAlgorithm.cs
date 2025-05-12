using System.Collections.Concurrent;
using System.Text.Json;
using MathNet.Numerics.Statistics;
using Microsoft.Extensions.Logging;
using SolvitaireCore;
using SolvitaireGenetics.IO;
using SolvitaireIO;
using SolvitaireIO.Database;
using SolvitaireIO.Database.Models;
using SolvitaireIO.Database.Repositories;

namespace SolvitaireGenetics;

public abstract class GeneticAlgorithm<TChromosome, TParameters> : IGeneticAlgorithm
    where TChromosome : Chromosome, new() 
    where TParameters : GeneticAlgorithmParameters 

{
    public event Action<int, GenerationLog>? GenerationCompleted;
    public virtual event Action<AgentLog>? AgentCompleted;
    private Task _loggingTask = Task.CompletedTask; // Initially completed


    protected readonly TParameters Parameters;
    protected readonly TChromosome? ChromosomeTemplate;
    protected readonly Random Random = new();
    protected double CrossOverRate = 0.5; // Default crossover rate
    public int CurrentGeneration { get; protected set; }
    public List<TChromosome> Population { get; protected set; } = [];
    public GeneticAlgorithmLogger Logger { get; }

    // Fitness cache
    private readonly ConcurrentDictionary<string, double> _fitnessCache = new();

    protected GeneticAlgorithm(TParameters parameters)
    {
        CurrentGeneration = 0;
        Parameters = parameters;
        if (parameters.TemplateChromosome is TChromosome template)
            ChromosomeTemplate = template;

        Logger = new GeneticAlgorithmLogger<TChromosome>(parameters.OutputDirectory);
        Logger = parameters.LoggingType switch
        {
            LoggingType.Json => new GeneticAlgorithmLogger<TChromosome>(parameters.OutputDirectory),
            LoggingType.Database => new DatabaseGeneticAlgorithmLogger(null, parameters.OutputDirectory),
            _ => throw new NotSupportedException($"Logging type {parameters.LoggingType} is not supported.")
        };
        Logger.SubscribeToAlgorithm(this);
    }

    public abstract double EvaluateFitness(TChromosome chromosome, CancellationToken? cancellationToken = null);
    private double GetFitness(TChromosome chromosome, CancellationToken? cancellationToken = null)
    {
        // Serialize the chromosome to use as a cache key
        string chromosomeKey = chromosome.GetStableHash();

        // Use GetOrAdd to ensure thread-safe access to the cache
        return _fitnessCache.GetOrAdd(chromosomeKey, _ => EvaluateFitness(chromosome, cancellationToken));
    }

    public Chromosome RunEvolution(int generations, CancellationToken? cancellationToken = null)
    {
        if (Population.Count == 0)
            Population = InitializePopulation(cancellationToken);

        int endGeneration = CurrentGeneration + generations;
        for (; CurrentGeneration < endGeneration;)
        {
            Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: Generation {CurrentGeneration}: Evaluating population...");

            // Step 1: Select parents for the entire population
            int numberOfParents = Parameters.PopulationSize * 2; // Each child needs 2 parents
            var parents = TournamentSelection(Population, numberOfParents);

            // Step 2: Create the new population
            for (int i = 0; i < Parameters.PopulationSize; i++)
            {
                var parent1 = parents[i * 2];
                var parent2 = parents[i * 2 + 1];

                // Perform crossover and mutation
                var child = Chromosome.Crossover(parent1, parent2, CrossOverRate);
                child = Chromosome.Mutate(child, Parameters.MutationRate);

                Population[i] = child;
            }

            CurrentGeneration++;

            _loggingTask.Wait(); // Wait for the previous logging task to complete if it hasn't finished

            // Step 3: Evaluate fitness for the new population
            Parallel.ForEach(Population, chromosome =>
               {
                   chromosome.Fitness = GetFitness(chromosome, cancellationToken);
               });

            // Step 4: Sort the new population by fitness (descending)
            Population = Population.OrderByDescending(chromosome => chromosome.Fitness).ToList();

            // Step 5: Start logging the current generation asynchronously
            
            _loggingTask = LogPopulationAsync(Population.ToList(), CurrentGeneration);

            if (cancellationToken?.IsCancellationRequested == true)
            {
                Console.WriteLine("Evolution process was cancelled.");
                break;
            }
        }

        // Ensure the final logging task is complete before returning
        _loggingTask.Wait();

        // Return the best chromosome from the last generation
        var bestChromosome = Population[0];
        bestChromosome.Fitness = GetFitness(bestChromosome, cancellationToken);
        return bestChromosome;
    }

    /// <summary>
    /// Grab a random sample of size _tournamentSize from the population and return the best one
    /// </summary>
    /// <summary>
    /// Selects parents for the entire evolution process using tournament selection.
    /// </summary>
    protected List<TChromosome> TournamentSelection(List<TChromosome> population, int numberOfParents)
    {
        var selectedParents = new List<TChromosome>(numberOfParents);

        // Shuffle the population to ensure fairness
        var shuffledPopulation = population.OrderBy(_ => Random.Next()).ToList();
        int currentIndex = 0;

        // Create tournaments
        for (int i = 0; i < numberOfParents; i++)
        {
            var tournament = new List<TChromosome>(Parameters.TournamentSize);

            // Select TournamentSize chromosomes from the shuffled population
            for (int j = 0; j < Parameters.TournamentSize; j++)
            {
                // If we reach the end of the shuffled population, reshuffle and reset the index
                if (currentIndex >= shuffledPopulation.Count)
                {
                    shuffledPopulation = population.OrderBy(_ => Random.Next()).ToList();
                    currentIndex = 0;
                }

                tournament.Add(shuffledPopulation[currentIndex]);
                currentIndex++;
            }

            // Select the winner based on fitness
            var winner = tournament
                .OrderByDescending(chromosome => chromosome.Fitness)
                .First();

            selectedParents.Add(winner);
        }

        return selectedParents;
    }

    /// <summary>
    /// Creates a random population of chromosomes with random weights.
    /// </summary>
    /// <returns></returns>
    public List<TChromosome> InitializePopulation(CancellationToken? cancellationToken = null)
    {
        // Check if we have a last generation to load
        var lastGeneration = Logger.LoadLastGeneration(out int generationNumber)
            .Select(p => p.Chromosome.Chromosome as TChromosome)
            .Where(p => p != null)
            .ToList();

        if (lastGeneration.Count == Parameters.PopulationSize)
        {
            // If we have a last generation, use it to initialize the population
            CurrentGeneration = generationNumber;
            return Population = lastGeneration;
        }

        List<TChromosome> population;

        // If we have a chromosome template, use it to create 10% of the population and mutate them
        // The other 90% will be random chromosomes crossed over with the template. 
        if (ChromosomeTemplate != null)
        {
            int copiesOfBest = (int)Math.Ceiling(Parameters.PopulationSize * Parameters.TemplateInitialRatio);
            var best = Enumerable.Range(0, copiesOfBest).Select(_ => ChromosomeTemplate)
                .Select(b => Chromosome.Mutate(b, Parameters.MutationRate));

            population =  Enumerable.Range(0, Parameters.PopulationSize - copiesOfBest)
                .Select(_ => Chromosome.CreateRandom<TChromosome>(Random)
                        .CrossOver(ChromosomeTemplate) // Temporary? Cross over with best.
                ).Concat(best)
                .ToList();
        }
        // If we don't have a last generation or a template, create a new random population
        else
        {
            population = Enumerable.Range(0, Parameters.PopulationSize)
                .Select(_ => Chromosome.CreateRandom<TChromosome>(Random))
                .ToList();
        }

        Parallel.ForEach(population, chromosome =>
               {
                   chromosome.Fitness = GetFitness(chromosome, cancellationToken);
               });

        // Sort the population by fitness (descending)
        population = population.OrderByDescending(chromosome => chromosome.Fitness).ToList();

        // Log the population and return. 
        _loggingTask = LogPopulationAsync(population.ToList(), CurrentGeneration);
        return Population = population;
    }

    private async Task LogPopulationAsync(List<TChromosome> population, int generation)
    {
        // Create ChromosomeLogs for best, average, and std chromosomes
        var bestChromosome = ChromosomeLog.FromChromosome(population[0]);
        var averageChromosome = ChromosomeLog.FromChromosome(Chromosome.GetAverageChromosome(population));
        var stdChromosome = ChromosomeLog.FromChromosome(Chromosome.GetStandardDeviationChromosome(population));

        // Log the best, average, and std chromosomes asynchronously
        await Task.Run(() =>
        {
            Logger.LogChromosome(bestChromosome);
            Logger.LogChromosome(averageChromosome);
            Logger.LogChromosome(stdChromosome);
        });

        // Flush agent logs asynchronously
        await Task.Run(() =>
        {
            Logger.FlushAgentLogs(generation, population);
        });

        // Create the GenerationLog
        var species = Chromosome.KMeansSpeciationElbow(population);
        var generationLog = new GenerationLog
        {
            Generation = generation,
            BestFitness = population.Max(c => c.Fitness),
            AverageFitness = population.Average(c => c.Fitness),
            StdFitness = population.Select(c => c.Fitness).StandardDeviation(),
            AveragePairwiseDiversity = Chromosome.AveragePairwiseDiversity(population),
            VarianceFromAverageChromosome = Chromosome.VarianceFromCentroid(population),
            SpeciesCount = species.Count,
            IntraSpeciesDiversity = Chromosome.IntraSpeciesDiversity(species),
            InterSpeciesDiversity = Chromosome.InterSpeciesDiversity(species),
            BestChromosomeId = bestChromosome.StableHash,
            AverageChromosomeId = averageChromosome.StableHash,
            StdChromosomeId = stdChromosome.StableHash,
            GenerationFinishedTime = DateTime.UtcNow,
            BestChromosome = bestChromosome,
            AverageChromosome = averageChromosome,
            StdChromosome = stdChromosome,
        };


        // Fire the GenerationCompleted event asynchronously
        if (GenerationCompleted != null)
        {
            GenerationCompleted.Invoke(generation, generationLog);
        }
    }
}

