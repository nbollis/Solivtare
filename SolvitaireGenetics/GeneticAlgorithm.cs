using System.Collections.Concurrent;
using MathNet.Numerics.Statistics;
using SolvitaireCore;
using SolvitaireGenetics.IO;
using SolvitaireIO.Database;
using SolvitaireIO.Database.Models;

namespace SolvitaireGenetics;

public abstract class GeneticAlgorithm<TChromosome, TParameters, TAgent> : IGeneticAlgorithm
    where TChromosome : Chromosome, new() 
    where TParameters : GeneticAlgorithmParameters
    where TAgent : class, IGeneticAgent<TChromosome>
{
    public event Action<int, GenerationLog>? GenerationCompleted;
    public virtual event Action<AgentLog>? AgentCompleted;
    private Task _loggingTask = Task.CompletedTask; // Initially completed


    protected readonly TParameters Parameters;
    protected readonly TChromosome? ChromosomeTemplate;
    protected readonly Random Random = new();
    protected double CrossOverRate = 0.5; // Default crossover rate
    protected ISelectionStrategy<TAgent, TChromosome> SelectionStrategy;
    protected IReproductionStrategy<TAgent, TChromosome> ReproductionStrategy;

    public int CurrentGeneration { get; protected set; }
    public List<TAgent> Population { get; protected set; } = [];
    public GeneticAlgorithmLogger Logger { get; }

    // Fitness cache
    protected readonly ConcurrentDictionary<string, double> FitnessCache = new();

    protected GeneticAlgorithm(TParameters parameters)
    {
        CurrentGeneration = 0;
        Parameters = parameters;
        if (parameters.TemplateChromosome is TChromosome template)
            ChromosomeTemplate = template;


        SelectionStrategy = SelectionStrategyFactory.Create<TAgent, TChromosome>(parameters.SelectionStrategy);
        ReproductionStrategy = ReproductionStrategyFactory.Create<TAgent, TChromosome>(parameters.ReproductionStrategy);

        Logger = parameters.LoggingType switch
        {
            LoggingType.Json => new GeneticAlgorithmLogger<TChromosome>(parameters.OutputDirectory),
            LoggingType.Database => new DatabaseGeneticAlgorithmLogger(new DbContextFactory(parameters.OutputDirectory)),
            _ => throw new NotSupportedException($"Logging type {parameters.LoggingType} is not supported.")
        };
        Logger.SubscribeToAlgorithm(this);
    }

    public virtual TAgent CreateFromChromosome(TChromosome chromosome)
    {
        return chromosome.ToGeneticAgent<TChromosome, TAgent>();
    }

    public abstract double EvaluateFitness(TAgent agent, CancellationToken? cancellationToken = null);
    private double GetFitness(TAgent chromosome, CancellationToken? cancellationToken = null)
    {
        // Serialize the chromosome to use as a cache key
        string chromosomeKey = chromosome.GetStableHash();

        // Use GetOrAdd to ensure thread-safe access to the cache
        return FitnessCache.GetOrAdd(chromosomeKey, _ => EvaluateFitness(chromosome, cancellationToken));
    }

    public virtual void EvaluatePopulation(CancellationToken? cancellationToken = null)
    {
        var uniqueChromosomes = Population
            .GroupBy(c => c.GetStableHash())
            .Select(g => g.First())
            .ToList();

        Parallel.ForEach(uniqueChromosomes, chromosome =>
        {
            // This will cache the fitness for each unique chromosome
            chromosome.Fitness = GetFitness(chromosome, cancellationToken);
        });

        // Assign cached fitness to all chromosomes in the population
        foreach (var agent in Population)
        {
            agent.Fitness = GetFitness(agent, cancellationToken);
        }
    }

    public Chromosome RunEvolution(int generations, CancellationToken? cancellationToken = null)
    {
        if (Population.Count == 0)
            Population = InitializePopulation(cancellationToken);

        int endGeneration = CurrentGeneration + generations;
        for (; CurrentGeneration < endGeneration;)
        {
            Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: Generation {CurrentGeneration}: Evaluating population...");

            // Step 1: Select parents using the strategy
            int numberOfParents = Population.Count * 2; // Or as needed by your strategy
            var parents = SelectionStrategy.Select(Population, numberOfParents, Parameters, Random);

            // Step 2: Create the new population using the reproduction strategy
            Population = ReproductionStrategy.Reproduce(parents, Parameters.PopulationSize, CrossOverRate, Parameters.MutationRate, Random);

            CurrentGeneration++;

            _loggingTask.Wait(); // Wait for the previous logging task to complete if it hasn't finished

            // Step 3: Evaluate the fitness of the population
            EvaluatePopulation(cancellationToken);

            // Step 4: Sort the new population by fitness (descending)
            Population = Population.OrderByDescending(chromosome => chromosome.Fitness).ToList();

            // Step 5: Start logging the current generation asynchronously
            _loggingTask = LogPopulationAsync(Population.ToList(), CurrentGeneration);

            if (ThanosSnapTriggered)
                PerfectlyBalanced();

            if (cancellationToken?.IsCancellationRequested == true)
            {
                Console.WriteLine("Evolution process was cancelled.");
                break;
            }
        }

        // Ensure the final logging task is complete before returning
        _loggingTask.Wait();

        // Return the best chromosome from the last generation
        var bestAgent = Population[0];
        bestAgent.Fitness = GetFitness(bestAgent, cancellationToken);
        return bestAgent.Chromosome;
    }

    /// <summary>
    /// Creates a random population of chromosomes with random weights.
    /// </summary>
    /// <returns></returns>
    public List<TAgent> InitializePopulation(CancellationToken? cancellationToken = null)
    {
        // Check if we have a last generation to load
        var lastGeneration = Logger.LoadLastGeneration(out int generationNumber)
            .Select(p => p.Chromosome.Chromosome as TChromosome)
            .Where(p => p != null)
            .Select(CreateFromChromosome!)
            .ToList();

        if (lastGeneration.Count == Parameters.PopulationSize)
        {
            // If we have a last generation, use it to initialize the population
            CurrentGeneration = generationNumber;
            return Population = lastGeneration;
        }

        List<TAgent> population;

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
                .Select(CreateFromChromosome)
                .ToList();
        }
        // If we don't have a last generation or a template, create a new random population
        else
        {
            population = Enumerable.Range(0, Parameters.PopulationSize)
                .Select(_ => Chromosome.CreateRandom<TChromosome>(Random))
                .Select(CreateFromChromosome)
                .ToList();
        }

        Population = population;
        EvaluatePopulation(cancellationToken);

        // Sort the population by fitness (descending)
        population = population.OrderByDescending(chromosome => chromosome.Fitness).ToList();

        // Log the population and return. 
        _loggingTask = LogPopulationAsync(population.ToList(), CurrentGeneration);
        return Population = population;
    }

    private async Task LogPopulationAsync(List<TAgent> population, int generation)
    {
        // Create ChromosomeLogs for best, average, and std chromosomes
        var bestChromosome = ChromosomeLog.FromChromosome(population[0].Chromosome);
        var averageChromosome = ChromosomeLog.FromChromosome(population.GetAverageChromosome<TAgent, TChromosome>());
        var stdChromosome = ChromosomeLog.FromChromosome(population.GetStandardDeviationChromosome<TAgent, TChromosome>());

        // Flush agent logs asynchronously
        await Task.Run(() =>
        {
            Logger.FlushAgentLogs(generation, population.Select(p => p.Chromosome).ToList());
        });

        // TODO: Make many of these method operate on agent instead of chromosome.
        // Create the GenerationLog
        var species = Chromosome.KMeansSpeciationElbow(population.Select(p => p.Chromosome).ToList());
        var generationLog = new GenerationLog
        {
            Generation = generation,
            BestFitness = population.Max(c => c.Fitness),
            AverageFitness = population.Average(c => c.Fitness),
            StdFitness = population.Select(c => c.Fitness).StandardDeviation(),
            AveragePairwiseDiversity = Chromosome.AveragePairwiseDiversity(population.Select(p => p.Chromosome).ToList()),
            VarianceFromAverageChromosome = Chromosome.VarianceFromCentroid(population.Select(p => p.Chromosome).ToList()),
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
        var generationLogging = Task.Run(() => Logger.LogGenerationInfo(generationLog));
        if (GenerationCompleted != null)
        {
            GenerationCompleted.Invoke(generation, generationLog); // GUI is listening
        }

        await generationLogging;
    }

    #region Thanos

    /// <summary>
    /// Flag to indicate if the Thanos snap should be triggered at the end of the generation.
    /// </summary>
    public bool ThanosSnapTriggered { get; set; }

    private void PerfectlyBalanced()
    {
        // Keep the top half (fittest) of the population
        int survivors = Population.Count / 2;
        Population = Population.Take(survivors).ToList();

        // Optionally reset the flag if you want it to be a one-time event
        ThanosSnapTriggered = false;
    }

    #endregion
}

