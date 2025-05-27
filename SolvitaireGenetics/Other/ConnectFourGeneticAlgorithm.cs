using System.Collections.Concurrent;
using MathNet.Numerics.RootFinding;
using SolvitaireCore;
using SolvitaireCore.ConnectFour;
using SolvitaireIO.Database.Models;
using System.Text.Json;
using System.Text;

namespace SolvitaireGenetics;

public class ConnectFourGeneticAlgorithmParameters : GeneticAlgorithmParameters
{
    public double RandomAgentRatio { get; set; } = 0.4;
    public int GamesPerPairing { get; set; } = 10;
    public override void SaveToFile(string filePath)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }
}

public class ConnectFourGeneticAlgorithm : GeneticAlgorithm<ConnectFourChromosome, ConnectFourGeneticAlgorithmParameters, ConnectFourGeneticAgent>
{
    public override event Action<AgentLog>? AgentCompleted;
    public ConnectFourGeneticAlgorithm(ConnectFourGeneticAlgorithmParameters parameters) : base(parameters)
    {
    }

    public override void EvaluatePopulation(CancellationToken? cancellationToken = null)
    {
        // Use Swiss tournament evaluation
        EvaluatePopulationSwissTournament(Parameters.TournamentSize, Parameters.GamesPerPairing, cancellationToken);
    }

    public override double EvaluateFitness(ConnectFourGeneticAgent agent, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException("This should never be hit as EvaluatePopulation is used instead.");
    }


    public void EvaluatePopulationSwissTournament(int rounds, int gamesPerPairing, CancellationToken? cancellationToken = null)
    {
        // Initialize scores
        var scores = Population.ToDictionary(agent => agent, _ => 0.0);
        var movesMade = Population.ToDictionary(agent => agent, _ => 0);
        var gamesWon = Population.ToDictionary(agent => agent, _ => 0);
        double maxScore = rounds * gamesPerPairing; // Max score per agent
        var localResults = new ConcurrentBag<(ConnectFourGeneticAgent, double, int, int)>();
        var pairings = new List<(ConnectFourGeneticAgent, ConnectFourGeneticAgent, int, int)>(Population.Count / 2);

        localResults.Clear();
        pairings.Clear();

        for (int round = 0; round < rounds; round++)
        {
            localResults.Clear();
            pairings.Clear();

            if (round == 0)
            {
                // First round: each agent plays against a random agent
                Parallel.ForEach(Population, agent =>
                {
                    var randomAgent = new RandomAgent<ConnectFourGameState, ConnectFourMove>();
                    var result = PlayGames(agent, randomAgent, gamesPerPairing);

                    localResults.Add((agent, result.ScoreA, result.GamesWonA, result.MovesMade));
                    // No need to log the random agent's stats
                });
            }
            else
            {
                // Swiss pairing for subsequent rounds
                var sorted = Population.OrderByDescending(a => scores[a]).ToList();
                for (int i = 0; i < sorted.Count - 1; i += 2)
                {
                    var agentA = sorted[i];
                    var agentB = sorted[i + 1];
                    pairings.Add((agentA, agentB, i, i + 1));
                }

                Parallel.ForEach(pairings, pairing =>
                {
                    var (agentA, agentB, _, _) = pairing;
                    var result = PlayGames(agentA, agentB, gamesPerPairing);

                    localResults.Add((agentA, result.ScoreA, result.GamesWonA, result.MovesMade));
                    localResults.Add((agentB, result.ScoreB, result.GamesWonB, result.MovesMade));
                });
            }

            // Aggregate results
            foreach (var (agent, score, gamesWonCount, moves) in localResults)
            {
                scores[agent] += score;
                movesMade[agent] += moves;
                gamesWon[agent] += gamesWonCount;
            }
        }

        // --- MinimaxAgent round ---  
        int minimaxGames = gamesPerPairing * 2; // Magic Number: 
        maxScore += minimaxGames; // Each agent gets minimaxGames more possible points  

        Parallel.ForEach(Population, agent =>
        {
            var depths = new[] { 1, 3, 5 };
            var localScores = new double[depths.Length];
            var localMovesMade = new int[depths.Length];
            var localGamesWon = new int[depths.Length];

            Parallel.For(0, depths.Length, depthIndex =>
            {
                var depth = depths[depthIndex];
                var minimaxAgent = new MinimaxAgent<ConnectFourGameState, ConnectFourMove>(
                    new ConnectFourHeuristicEvaluator(), maxDepth: depth);

                var result = PlayGames(agent, minimaxAgent, minimaxGames / depths.Length);

                localScores[depthIndex] = result.ScoreA;
                localMovesMade[depthIndex] = result.MovesMade;
                localGamesWon[depthIndex] = result.GamesWonA;
            });

            lock (scores) // Dictionaries are not thread-safe for writes  
            {
                scores[agent] += localScores.Sum();
                movesMade[agent] += localMovesMade.Sum();
                gamesWon[agent] += localGamesWon.Sum();
            }
        });
        // --- End MinimaxAgent round ---

        // Assign fitness
        foreach (var agent in Population)
        {
            var fitness = scores[agent] / maxScore; // Normalize fitness

            agent.Fitness = fitness; // Update the agent's chromosome fitness

            // Update the fitness cache and assume single threaded. 
            FitnessCache[agent.Chromosome.GetStableHash()] = fitness;

            // Log the agent's performance
            AgentCompleted?.Invoke(new AgentLog
            {
                Generation = CurrentGeneration,
                Fitness = fitness,
                GamesPlayed = (int)maxScore,
                MovesMade = movesMade[agent],
                GamesWon = gamesWon[agent],
                Chromosome = ChromosomeLog.FromChromosome(agent.Chromosome)
            });
        }
    }

    private (double ScoreA, double ScoreB, int GamesWonA, int GamesWonB, int MovesMade) PlayGames(
        IAgent<ConnectFourGameState, ConnectFourMove> agentA,
        IAgent<ConnectFourGameState, ConnectFourMove> agentB,
        int gamesPerPairing)
    {
        double scoreA = 0, scoreB = 0;
        int gamesWonA = 0, gamesWonB = 0;
        int movesPlayed = 0;


        for (int i = 0; i < gamesPerPairing; i++)
        {
            
            // Alternate who is Player 1 and Player 2
            IAgent<ConnectFourGameState, ConnectFourMove> player1, player2;
            if (i % 2 == 0)
            {
                player1 = agentA;
                player2 = agentB;
            }
            else
            {
                player1 = agentB;
                player2 = agentA;
            }

            // Assumes ConnectFourGameState.CurrentPlayer = 1 by default
            // index 1 = player1, 2 = player2
            var agents = new[] { null, player1, player2 }; 
            var gameState = new ConnectFourGameState();
            while (gameState is { IsGameWon: false, IsGameDraw: false })
            {
                var currentAgent = agents[gameState.CurrentPlayer];
                var move = currentAgent!.GetNextAction(gameState);
                gameState.ExecuteMove(move);
                movesPlayed++;
            }

            // Attribute the result to the correct agent
            if (gameState.WinningPlayer == 1)
            {
                // Player 1 won
                if (i % 2 == 0)
                {
                    scoreA += 1;
                    gamesWonA++;
                }
                else
                {
                    scoreB += 1;
                    gamesWonB++;
                }
            }
            else if (gameState.WinningPlayer == 2)
            {
                // Player 2 won
                if (i % 2 == 0)
                {
                    scoreB += 1;
                    gamesWonB++;
                }
                else
                {
                    scoreA += 1;
                    gamesWonA++;
                }
            }
            else
            {
                // Draw
                scoreA += 0.5;
                scoreB += 0.5;
            }

            agentA.ResetState();
            agentB.ResetState();
        }

        // Divide by two because we tracked both players moves
        return (scoreA, scoreB, gamesWonA, gamesWonB, movesPlayed / 2);
    }
}
