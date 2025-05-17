using SolvitaireCore;
using SolvitaireIO;
using SolvitaireIO.Database.Models;
using System;

namespace SolvitaireGenetics;

public class GeneticSolitaireAlgorithm : GeneticAlgorithm<SolitaireChromosome, SolitaireGeneticAlgorithmParameters>
{
    private readonly List<StandardDeck> _predefinedDecks = new();
    private readonly IDeckFile? _deckFile;
    public override event Action<AgentLog>? AgentCompleted;

    public GeneticSolitaireAlgorithm(SolitaireGeneticAlgorithmParameters parameters)
        : base(parameters)
    {
        _predefinedDecks = new();
        // Deserialize predefined decks if provided
        if (parameters.DecksToUse is not null)
        {
            _deckFile = new DeckStatisticsFile(parameters.DecksToUse);
            _predefinedDecks.AddRange(_deckFile.ReadAllDecks());
            _predefinedDecks.ForEach(p => p.FlipAllCardsDown());
        }

        Random rand = new(42);
        while (_predefinedDecks.Count < parameters.MaxGamesPerGeneration)
        {
            var deck = new StandardDeck(rand.Next(0, 42));
            for (int i = 0; i < rand.Next(0, 10); i++)
                deck.Shuffle();
            _predefinedDecks.Add(deck);
        }

        _predefinedDecks = _predefinedDecks.OrderBy(_ => Random.Next()).ToList();

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
            StandardDeck deck = _predefinedDecks[gamesPlayed];
            //agent.ResetState();
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
                else if (decision.Move == null)
                {
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
        best.MutableStatsByName[SolitaireChromosome.LegalMoveWeightName] = 1.0252;
        best.MutableStatsByName[SolitaireChromosome.FoundationWeightName] = 6.8471;
        best.MutableStatsByName[SolitaireChromosome.WasteWeightName] = -1.6529;
        best.MutableStatsByName[SolitaireChromosome.StockWeightName] = -0.6287;
        best.MutableStatsByName[SolitaireChromosome.CycleWeightName] = -0.0407;
        best.MutableStatsByName[SolitaireChromosome.EmptyTableauWeightName] = 0.0713;
        best.MutableStatsByName[SolitaireChromosome.FaceUpTableauWeightName] = 0.1376;
        best.MutableStatsByName[SolitaireChromosome.FaceDownTableauWeightName] = 1.9865;
        best.MutableStatsByName[SolitaireChromosome.ConsecutiveFaceUpTableauWeightName] = 4.7181;
        best.MutableStatsByName[SolitaireChromosome.FaceUpBottomCardTableauWeightName] = 1.5819;
        best.MutableStatsByName[SolitaireChromosome.KingIsBottomCardTableauWeightName] = 1.7532;
        best.MutableStatsByName[SolitaireChromosome.AceInTableauWeightName] = -1.1593;
        best.MutableStatsByName[SolitaireChromosome.FoundationRangeWeightName] = -0.4776;
        best.MutableStatsByName[SolitaireChromosome.FoundationDeviationWeightName] = -1.1048;

        // Skipping Games
        best.MutableStatsByName[SolitaireChromosome.MoveCountScalarName] = -0.4904;
        best.MutableStatsByName[SolitaireChromosome.Skip_FoundationCount] = -1.8681;
        best.MutableStatsByName[SolitaireChromosome.Skip_LegalMoveCount] = -1.7573;
        best.MutableStatsByName[SolitaireChromosome.Skip_ThresholdWeightName] = -1.6683;
        best.MutableStatsByName[SolitaireChromosome.Skip_TopWasteIsUseful] = -0.0617;
        best.MutableStatsByName[SolitaireChromosome.Skip_CycleWeight] = -0.3949;
        best.MutableStatsByName[SolitaireChromosome.Skip_StockWeight] = -1.7414;
        best.MutableStatsByName[SolitaireChromosome.Skip_WasteWeight] = 0.6467;
        best.MutableStatsByName[SolitaireChromosome.Skip_EmptyTableauCount] = -2.0881;
        best.MutableStatsByName[SolitaireChromosome.Skip_FaceUpTableauCount] = -0.2129;
        best.MutableStatsByName[SolitaireChromosome.Skip_FaceDownTableauCount] = 1.079;
        return best;
    }
}