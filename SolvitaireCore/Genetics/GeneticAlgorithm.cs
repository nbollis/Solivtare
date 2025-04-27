using Microsoft.Extensions.Logging;

namespace SolvitaireCore;

public abstract class GeneticAlgorithm<TChromosome> where TChromosome : Chromosome<TChromosome>
{
    private readonly ILogger<GeneticAlgorithm<TChromosome>> _logger;
    private readonly int _populationSize;
    private readonly Random _random = new();
    private readonly double _mutationRate;
    private readonly int _tournamentSize;
    // Fitness cache
    private readonly Dictionary<int, double> _fitnessCache = new();

    public GeneticAlgorithm(int populationSize, double mutationRate, int tournamentSize)
    {
        _populationSize = populationSize;
        _mutationRate = mutationRate;
        _tournamentSize = tournamentSize;
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
            _logger.LogInformation($"Generation {generation}: Evaluating population...");

            population = EvolvePopulation(population, out List<double> fitness); 

            double bestFitness = fitness[0];
            TChromosome bestChromosome = population[0];
            TChromosome averageChromosome = Chromosome<TChromosome>.GetAverageChromosome(population);

            _logger.LogInformation($"Generation {generation}: Best fitness = {bestFitness}, BestChromosome = {bestChromosome.SerializeWeights()}, AverageChromosome = {averageChromosome.SerializeWeights()}");
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

        while (newPopulation.Count < _populationSize)
        {
            var parent1 = TournamentSelection(currentPopulation);
            var parent2 = TournamentSelection(currentPopulation);

            var child = Chromosome<TChromosome>.Crossover(parent1, parent2);
            child = Chromosome<TChromosome>.Mutate(child, _mutationRate);

            newPopulation.Add(child);
        }

        // Sort the new population by fitness (descending)
        newPopulation = newPopulation.OrderByDescending(GetFitness).ToList();

        // Cache fitness values for the new population
        fitness.AddRange(newPopulation.Select(GetFitness));

        return newPopulation;
    }

    /// <summary>
    /// Grab a random sample of size _tournamentSize from the population and return the best one.
    /// </summary>
    protected TChromosome TournamentSelection(List<TChromosome> population)
    {
        TChromosome bestChromosome = null!;

        // select a random sample of size _tournamentSize from the population
        var tournament = new List<TChromosome>();
        for (int i = 0; i < _tournamentSize; i++)
        {
            tournament.Add(population[_random.Next(population.Count)]);
        }

        // find the best chromosome in the tournament
        foreach (var chromosome in tournament)
        {
            double localFitness = GetFitness(chromosome);
            if (localFitness > chromosome.Fitness)
            {
                bestChromosome = chromosome;
            }
        }

        return bestChromosome;
    }

    /// <summary>
    /// Creates a random population of chromosomes with random weights.
    /// </summary>
    /// <returns></returns>
    protected List<TChromosome> InitializePopulation()
    {
        return Enumerable.Range(0, _populationSize)
            .Select(_ => Chromosome<TChromosome>.CreateRandom(_random))
            .ToList();
    }
}

public class GeneticSolitaireAlgorithm : GeneticAlgorithm<SolitaireChromosome>
{
    private readonly int _maxMovesPerAgent;
    private readonly int _maxGamesPerAgent;

    public GeneticSolitaireAlgorithm(int populationSize, double mutationRate, int tournamentSize, int maxMovesPerAgent, int maxGamesPerAgent) : base(populationSize, mutationRate, tournamentSize)
    {
        _maxMovesPerAgent = maxMovesPerAgent;
        _maxGamesPerAgent = maxGamesPerAgent;
    }

    protected override double EvaluateFitness(SolitaireChromosome chromosome)
    {
        // TODO: Make this more generic to support different agents. 
        var evaluator = new GeneticSolitaireEvaluator(chromosome);
        var agent = new MaxiMaxAgent(evaluator, 10);
        var deck = new StandardDeck();
        var gameState = new SolitaireGameState();

        int movesPlayed = 0;
        int gamesPlayed = 0;
        int gamesWon = 0;

        while (gamesPlayed < _maxGamesPerAgent && movesPlayed < _maxMovesPerAgent)
        {
            deck.Shuffle();
            agent.ResetState();
            gameState.Reset();
            gameState.DealCards(deck);

            gamesPlayed++;
            while (!gameState.IsGameWon)
            {
                var decision = agent.GetNextAction(gameState);
                if (decision.ShouldSkipGame)
                {
                    break;
                }
                else
                {
                    gameState.ExecuteMove(decision.Move!);
                }

                movesPlayed++;
            }

            if (gameState.IsGameWon)
            {
                gamesWon++;
            }
        }

        // Calculate fitness based on the number of games won and moves played
        double fitness = (double)gamesWon / gamesPlayed;
        if (gamesPlayed > 0)
        {
            fitness -= (double)movesPlayed / (gamesPlayed * _maxMovesPerAgent);
        }

        chromosome.Fitness = fitness;
        return fitness;
    }
}

