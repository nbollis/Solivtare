using SolvitaireCore;
using SolvitaireIO;
using SolvitaireIO.Database.Models;
using System;

namespace SolvitaireGenetics;

public class GeneticSolitaireAlgorithm : GeneticAlgorithm<SolitaireChromosome, SolitaireGeneticAlgorithmParameters, SolitaireGeneticAgent>
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

    public override double EvaluateFitness(SolitaireGeneticAgent agent, CancellationToken? cancellationToken = null)
    {
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

            // Play a game until it is skipped, or you run out of moves. 
            while (!gameState.IsGameWon)
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
                if (decision.IsTerminatingMove)
                {
                    Console.WriteLine("Game Skipped.");
                    break;
                }
                else if (decision == null)
                {
                }
                else
                {
                    gameState.ExecuteMove(decision);
                }

                movesPlayed++;
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
            Chromosome = ChromosomeLog.FromChromosome(agent.Chromosome)
        };

        agent.Chromosome.Fitness = fitness;
        AgentCompleted?.Invoke(agentLog);
        return fitness;
    }

}