using System.Diagnostics;
using SolvitaireCore;
using SolvitaireIO;

namespace SolvitaireGenetics;

public class GeneticSolitaireAlgorithm : GeneticAlgorithm<SolitaireChromosome, SolitaireGeneticAlgorithmParameters>
{
    private readonly int _maxMovesPerAgent;
    private readonly int _maxGamesPerAgent;
    private readonly List<StandardDeck> _predefinedDecks = new();
    private readonly IDeckFile? _deckFile;
    private readonly SolitaireGeneticAlgorithmParameters _parameters;
    public override event Action<AgentLog>? AgentCompleted;

    public GeneticSolitaireAlgorithm(SolitaireGeneticAlgorithmParameters parameters)
        : base(parameters)
    {
        _parameters = parameters;
        _maxMovesPerAgent = parameters.MaxMovesPerGeneration;
        _maxGamesPerAgent = parameters.MaxGamesPerGeneration;

        // Deserialize predefined decks if provided
        if (parameters.DecksToUse is not null)
        {
            _deckFile = new DeckStatisticsFile(parameters.DecksToUse);
            _predefinedDecks = _deckFile.ReadAllDecks();
            _predefinedDecks.ForEach(p => p.FlipAllCardsDown());
        }

        // Be sure to flush the deck file at the end of the generation.
        GenerationCompleted += (a, b) =>
        {
            if (_deckFile is DeckStatisticsFile stats)
            {
                stats.Flush();
            }
        };
    }

    public override double EvaluateFitness(SolitaireChromosome chromosome)
    {
        var sw = Stopwatch.StartNew();
        // TODO: Make this more generic to support different agents. 
        var evaluator = new GeneticSolitaireEvaluator(chromosome);
        var agent = new MaxiMaxAgent(evaluator, 10);
        var gameState = new SolitaireGameState();

        int movesPlayed = 0;
        int gamesPlayed = 0;
        int gamesWon = 0;
        int foundationCards = 0;

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

                // Only record winnable decks for right now. 
                if (_deckFile is DeckStatisticsFile stats)
                {
                    stats.AddOrUpdateWinnableDeck(deck, gameState.MovesMade, gameState.IsGameWon);
                }
            }

            foundationCards += gameState.FoundationPiles.Sum(p => p.Count);
        }

        // Calculate fitness based on the number of games won and moves played
        double fitness = gamesWon;
        if (gamesPlayed > 0)
        {
            // Penalize making a lot of moves, but worsen the penalty as more games get played
            fitness -= 0.5 * movesPlayed / (gamesPlayed * _maxMovesPerAgent);

            // Add decimal equal to average % of deck that made it to the foundation
            fitness += foundationCards / (gamesPlayed * 52.0); 
        }
        else
        {
            fitness = -1;
        }

        var agentLog = new AgentLog
        {
            Chromosome = chromosome, Fitness = fitness, Generation = CurrentGeneration,
            GamesPlayed = gamesPlayed, MovesMade = movesPlayed, GamesWon = gamesWon
        };

        chromosome.Fitness = fitness;
        AgentCompleted?.Invoke(agentLog);
        Console.WriteLine(sw.Elapsed + "Elapsed");
        return fitness;
    }

    public static SolitaireChromosome BestSoFar()
    {
        var best = new SolitaireChromosome(Random.Shared);

        // Evaluating the Position
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

        // Skipping Games
        best.MutableStatsByName[SolitaireChromosome.MoveCountScalarName] = 0;
        best.MutableStatsByName[SolitaireChromosome.SkipLegalMoveWeightName] = 0;
        best.MutableStatsByName[SolitaireChromosome.SkipFoundationWeightName] = 0;
        best.MutableStatsByName[SolitaireChromosome.SkipThresholdWeightName] = 0;
        return best;
    }
}