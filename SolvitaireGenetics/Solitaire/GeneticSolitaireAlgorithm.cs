using SolvitaireCore;
using SolvitaireIO;
using SolvitaireIO.Database.Models;

namespace SolvitaireGenetics;

public class GeneticSolitaireAlgorithm : GeneticAlgorithm<SolitaireChromosome, SolitaireGeneticAlgorithmParameters>
{
    private readonly List<StandardDeck> _predefinedDecks = new();
    private readonly IDeckFile? _deckFile;
    public override event Action<AgentLog>? AgentCompleted;

    public GeneticSolitaireAlgorithm(SolitaireGeneticAlgorithmParameters parameters)
        : base(parameters)
    {
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

    public override double EvaluateFitness(SolitaireChromosome chromosome, CancellationToken? cancellationToken = null)
    {
        var evaluator = new GeneticSolitaireEvaluator(chromosome);
        var agent = new MaxiMaxAgent(evaluator, 8);
        var gameState = new SolitaireGameState();

        int movesPlayed = 0;
        int gamesPlayed = 0;
        int gamesWon = 0;
        int foundationCards = 0;
        int faceUpTableau = 0;

        for (int i = 0; i < Parameters.MaxGamesPerGeneration; i++)
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

            if (gamesPlayed >= Parameters.MaxGamesPerGeneration)
                break;

            int movesPlayedThisGame = 0;
            // Evenly divide the moves across the max allowed games. 
            while (!gameState.IsGameWon && movesPlayedThisGame <= (Parameters.MaxMovesPerGeneration - movesPlayed) / (Parameters.MaxGamesPerGeneration - gamesPlayed))
            {
                if (movesPlayed >= Parameters.MaxMovesPerGeneration)
                {
                    break;
                }
                // Check for cancellation
                if (cancellationToken?.IsCancellationRequested == true)
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
                movesPlayedThisGame++;
            }

            if (gameState.IsGameWon)
            {
                gamesWon++;
                Console.WriteLine("Game Won.");
            }

            // Update Deck Log. 
            if (_deckFile is DeckStatisticsFile stats)
            {
                stats.AddOrUpdateWinnableDeck(deck, gameState.MovesMade, gameState.IsGameWon);
            }

            foundationCards += gameState.FoundationPiles.Sum(p => p.Count);
            faceUpTableau += gameState.TableauPiles.Sum(pile => pile.Count(card => card.IsFaceUp));

            // Check for cancellation
            if (cancellationToken?.IsCancellationRequested == true)
            {
                break;
            }
        }

        double fitness = gamesWon;

        // Add a small gain for face up in tableau
        fitness += (0.01 * faceUpTableau / 52.0);

        // 1 game won = Add 0.5. Two Games won = Add 1. Three Games won = Add 1.5; 
        // Probably bad. 
        fitness += (0.5 * foundationCards / 52.0);

        var agentLog = new AgentLog
        {
            Generation = CurrentGeneration,
            Fitness = fitness,
            GamesPlayed = gamesPlayed,
            MovesMade = movesPlayed,
            GamesWon = gamesWon,
            Chromosome = ChromosomeLog.FromChromosome(chromosome)
        };

        chromosome.Fitness = fitness;
        AgentCompleted?.Invoke(agentLog);
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
        best.MutableStatsByName[SolitaireChromosome.FoundationRangeWeightName] = 0;
        best.MutableStatsByName[SolitaireChromosome.FoundationDeviationWeightName] = 0;

        // Skipping Games
        best.MutableStatsByName[SolitaireChromosome.MoveCountScalarName] = -0.9273;
        best.MutableStatsByName[SolitaireChromosome.Skip_FoundationCount] = -0.7625;
        best.MutableStatsByName[SolitaireChromosome.Skip_LegalMoveCount] = -1.0254;
        best.MutableStatsByName[SolitaireChromosome.Skip_ThresholdWeightName] = -1.3483;
        best.MutableStatsByName[SolitaireChromosome.Skip_TopWasteIsUseful] = 0.0;
        best.MutableStatsByName[SolitaireChromosome.Skip_CycleWeight] = 0;
        best.MutableStatsByName[SolitaireChromosome.Skip_StockWeight] = 0;
        best.MutableStatsByName[SolitaireChromosome.Skip_WasteWeight] = 0;
        best.MutableStatsByName[SolitaireChromosome.Skip_EmptyTableauCount] = 0;
        best.MutableStatsByName[SolitaireChromosome.Skip_FaceUpTableauCount] = 0;
        best.MutableStatsByName[SolitaireChromosome.Skip_FaceDownTableauCount] = 0;
        return best;
    }
}