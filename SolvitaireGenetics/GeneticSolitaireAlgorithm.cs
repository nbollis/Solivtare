using SolvitaireCore;
using SolvitaireIO;

namespace SolvitaireGenetics;

public class GeneticSolitaireAlgorithm : GeneticAlgorithm<SolitaireChromosome>
{
    private readonly int _maxMovesPerAgent;
    private readonly int _maxGamesPerAgent;
    private readonly List<StandardDeck> _predefinedDecks = new();

    public GeneticSolitaireAlgorithm(int populationSize, double mutationRate, int tournamentSize, int maxMovesPerAgent, 
        int maxGamesPerAgent, string outputDirectory, DeckFile? deckFile = null)
        : base(populationSize, mutationRate, tournamentSize, outputDirectory)
    {
        _maxMovesPerAgent = maxMovesPerAgent;
        _maxGamesPerAgent = maxGamesPerAgent;

        // Deserialize predefined decks if provided
        if (deckFile is not null)
        {
            _predefinedDecks = deckFile.ReadAllDecks();
            _predefinedDecks.ForEach(p => p.FlipAllCardsDown());
        }
    }

    protected override double EvaluateFitness(SolitaireChromosome chromosome)
    {
        // TODO: Make this more generic to support different agents. 
        var evaluator = new GeneticSolitaireEvaluator(chromosome);
        var agent = new MaxiMaxAgent(evaluator, 10);
        var gameState = new SolitaireGameState();

        int movesPlayed = 0;
        int gamesPlayed = 0;
        int gamesWon = 0;

        // multiple game loop
        while (gamesPlayed < _maxGamesPerAgent && movesPlayed < _maxMovesPerAgent)
        {
            StandardDeck deck = null!;
            // If we have predefined decks, use them in order. Otherwise create a new deck and shuffle it. 
            if (_predefinedDecks.Count == 0 || gamesPlayed >= _predefinedDecks.Count)
            {
                deck ??= new StandardDeck();
                deck.Shuffle();
            }
            else
            {
                deck = _predefinedDecks[gamesPlayed];
            }

            agent.ResetState();
            gameState.Reset();
            gameState.DealCards(deck);

            gamesPlayed++;

            // individual game loop
            while (!gameState.IsGameWon)
            {
                if (movesPlayed >= _maxMovesPerAgent)
                {
                    break;
                }

                var decision = agent.GetNextAction(gameState);

                // Handle possible actions. 
                if (decision.ShouldSkipGame)
                {
                    Console.WriteLine("Game Skipped.");
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
                Console.WriteLine("Game Won.");
            }
        }

        // Calculate fitness based on the number of games won and moves played
        double fitness = (double)gamesWon / gamesPlayed;
        if (gamesPlayed > 0)
        {
            fitness -= (double)movesPlayed / (gamesPlayed * _maxMovesPerAgent);
        }

        Logger.AccumulateAgentLog(CurrentGeneration, chromosome, fitness, gamesWon, movesPlayed, gamesPlayed);
        chromosome.Fitness = fitness;
        return fitness;
    }

    /// <summary>
    /// Creates a random population of chromosomes with random weights.
    /// </summary>
    /// <returns></returns>
    protected override List<SolitaireChromosome> InitializePopulation()
    {
        int copiesOfBest = PopulationSize / 10;
        var best = Enumerable.Range(0, copiesOfBest).Select(_ => BestSoFar())
            .Select(b => b.Mutate(MutationRate));

        return Enumerable.Range(0, PopulationSize - copiesOfBest)
            .Select(_ => Chromosome<SolitaireChromosome>.CreateRandom(Random)
                .CrossOver(BestSoFar()) // Temporary? Cross over with best.
            ).Concat(best)
            .ToList();
    }

    public static SolitaireChromosome BestSoFar()
    {
        var best = new SolitaireChromosome(Random.Shared);
        best.MutableStatsByName[SolitaireChromosome.LegalMoveWeightName] = 0.01;
        best.MutableStatsByName[SolitaireChromosome.FoundationWeightName] = 3.2;
        best.MutableStatsByName[SolitaireChromosome.WasteWeightName] = -0.01;
        best.MutableStatsByName[SolitaireChromosome.StockWeightName] = -0.02;
        best.MutableStatsByName[SolitaireChromosome.CycleWeightName] = -0.01;
        best.MutableStatsByName[SolitaireChromosome.EmptyTableauWeightName] = 0.3;
        best.MutableStatsByName[SolitaireChromosome.FaceUpTableauWeightName] = 0.5;
        best.MutableStatsByName[SolitaireChromosome.FaceDownTableauWeightName] = -0.75;
        best.MutableStatsByName[SolitaireChromosome.ConsecutiveFaceUpTableauWeightName] = 0.05;
        best.MutableStatsByName[SolitaireChromosome.FaceUpBottomCardTableauWeightName] = 0.2;
        best.MutableStatsByName[SolitaireChromosome.KingIsBottomCardTableauWeightName] = 0.4;
        best.MutableStatsByName[SolitaireChromosome.AceInTableauWeightName] = -1;
        best.MutableStatsByName[SolitaireChromosome.MoveCountWeightName] = 0;
        return best;
    }
}