using Microsoft.Extensions.Logging;
using SolvitaireCore;

namespace SolvitaireGenetics;

public class GeneticSolitaireAlgorithm : GeneticAlgorithm<SolitaireChromosome>
{
    private readonly int _maxMovesPerAgent;
    private readonly int _maxGamesPerAgent;

    public GeneticSolitaireAlgorithm(int populationSize, double mutationRate, int tournamentSize, int maxMovesPerAgent, int maxGamesPerAgent, ILogger<GeneticSolitaireAlgorithm> logger)
        : base(populationSize, mutationRate, tournamentSize, logger)
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