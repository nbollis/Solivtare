using System.Collections.Concurrent;
using MathNet.Numerics.Statistics;

namespace SolvitaireGenetics;

public abstract class GeneticAlgorithm<TChromosome, TParameters> : IGeneticAlgorithm
    where TChromosome : Chromosome, new() 
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
    public List<TChromosome> Population { get; protected set; } = [];

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


        Logger = new GeneticAlgorithmLogger<TChromosome>(parameters.OutputDirectory);
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
        if (Population.Count == 0)
            Population = InitializePopulation();

        int endGeneration = CurrentGeneration + generations;
        for (; CurrentGeneration < endGeneration;)
        {
            Console.WriteLine($"{DateTime.Now.ToShortTimeString()}: Generation {CurrentGeneration}: Evaluating population...");

            // Step 1: Select parents for the entire population
            int numberOfParents = PopulationSize * 2; // Each child needs 2 parents
            var parents = TournamentSelection(Population, numberOfParents);

            // Step 2: Create the new population
            for (int i = 0; i < PopulationSize; i++)
            {
                var parent1 = parents[i * 2];
                var parent2 = parents[i * 2 + 1];

                // Perform crossover and mutation
                var child = Chromosome.Crossover(parent1, parent2);
                child = Chromosome.Mutate(child, MutationRate);

                Population[i] = child;
            }

            CurrentGeneration++;

            // Step 3: Evaluate fitness for the new population
            Parallel.ForEach(Population, chromosome =>
            {
                chromosome.Fitness = GetFitness(chromosome);
            });

            // Step 4: Sort the new population by fitness (descending)
            Population = Population.OrderByDescending(chromosome => chromosome.Fitness).ToList();

            // Step 5: Log the generation information
            LogPopulation(Population);
        }

        // Return the best chromosome from the last generation
        var bestChromosome = Population[0];
        bestChromosome.Fitness = GetFitness(bestChromosome);
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
            var tournament = new List<TChromosome>(TournamentSize);

            // Select TournamentSize chromosomes from the shuffled population
            for (int j = 0; j < TournamentSize; j++)
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
    public List<TChromosome> InitializePopulation()
    {
        // Check if we have a last generation to load
        if (Logger is not null)
        {
            var lastGeneration = Logger.LoadLastGeneration(out int generationNumber);

            if (lastGeneration.Count == PopulationSize)
            {
                // If we have a last generation, use it to initialize the population
                CurrentGeneration = generationNumber;
                return lastGeneration;
            }
        }

        List<TChromosome> population;

        // If we have a chromosome template, use it to create 10% of the population and mutate them
        // The other 90% will be random chromosomes crossed over with the template. 
        if (ChromosomeTemplate != null)
        {
            int copiesOfBest = PopulationSize / 10;
            var best = Enumerable.Range(0, copiesOfBest).Select(_ => ChromosomeTemplate)
                .Select(b => Chromosome.Mutate(b, MutationRate));

            population =  Enumerable.Range(0, PopulationSize - copiesOfBest)
                .Select(_ => Chromosome.CreateRandom<TChromosome>(Random)
                        .CrossOver(ChromosomeTemplate) // Temporary? Cross over with best.
                ).Concat(best)
                .ToList();
        }
        // If we don't have a last generation or a template, create a new random population
        else
        {
            population = Enumerable.Range(0, PopulationSize)
                .Select(_ => Chromosome.CreateRandom<TChromosome>(Random))
                .ToList();
        }

        Parallel.ForEach(population, chromosome =>
        {
            chromosome.Fitness = GetFitness(chromosome);
        });

        // Sort the population by fitness (descending)
        population = population.OrderByDescending(chromosome => chromosome.Fitness).ToList();

        LogPopulation(population);
        return population;
    }

    private void LogPopulation(List<TChromosome> population)
    {
        var fitness = population.Select(chr => chr.Fitness).ToList();
        var generationLog = new GenerationLogDto
        {
            Generation = CurrentGeneration,
            BestFitness = fitness[0],
            AverageFitness = fitness.Average(),
            StdFitness = fitness.StandardDeviation(),
            BestChromosome = population[0],
            AverageChromosome =  Chromosome.GetAverageChromosome(population),
            StdChromosome = Chromosome.GetStandardDeviationChromosome(population)
        };

        GenerationCompleted?.Invoke(CurrentGeneration, generationLog); // Fire the GenerationCompleted event  
        Logger?.FlushAgentLogs(CurrentGeneration, Population);
    }
}

