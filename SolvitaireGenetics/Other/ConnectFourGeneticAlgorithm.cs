using SolvitaireCore;
using SolvitaireCore.ConnectFour;
using SolvitaireIO.Database.Models;
using System.Text.Json;

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

    public override double EvaluateFitness(ConnectFourGeneticAgent agent, CancellationToken? cancellationToken = null)
    {
        int totalPairings = Parameters.TournamentSize;
        int gamesAgainstRandomAgent = (int)(totalPairings * Parameters.RandomAgentRatio);
        int gamesAgainstOtherAgents = totalPairings - gamesAgainstRandomAgent;
        int maxScore = totalPairings * Parameters.GamesPerPairing; // 5 games per pairing  

        Random random = new Random();

        double totalScore = 0;
        int gamesWon = 0;
        int movesPlayed = 0;
        // Play games against RandomAgent  
        for (int i = 0; i < gamesAgainstRandomAgent; i++)
        {
            var randomAgent = new RandomAgent<ConnectFourGameState, ConnectFourMove>();
            if (cancellationToken?.IsCancellationRequested == true)
                break;

            var result = PlayGames(agent, randomAgent);
            totalScore += result.Score;
            gamesWon += result.GamesWon;
            movesPlayed += result.MovesMade;
        }

        // Play games against other random agents from the population  
        for (int i = 0; i < gamesAgainstOtherAgents; i++)
        {
            if (cancellationToken?.IsCancellationRequested == true)
                break;

            // Select a random agent from the population, ensuring it's not the same as the current agent
            var opponent = (ConnectFourGeneticAgent)Population[random.Next(Population.Count)].Clone();
            if (opponent == agent)  
            {
                i--;
                continue;
            }

            var result = PlayGames(agent, opponent);
            totalScore += result.Score;
            gamesWon += result.GamesWon;
            movesPlayed += result.MovesMade;
        }

        // Normalize fitness score  
        var fitness = totalScore / maxScore;

        AgentCompleted?.Invoke(new AgentLog
        {
            Generation = CurrentGeneration,
            Fitness = fitness,
            GamesPlayed = maxScore,
            MovesMade = movesPlayed,
            GamesWon = gamesWon,
            Chromosome = ChromosomeLog.FromChromosome(agent.Chromosome)
        });
        agent.Chromosome.Fitness = fitness; // Update the agent's chromosome fitness
        return fitness;
    }

    private (double Score, int GamesWon, int MovesMade) PlayGames(IAgent<ConnectFourGameState, ConnectFourMove> agent1, IAgent<ConnectFourGameState, ConnectFourMove> agent2)
    {
        double score = 0;
        int gamesWon = 0;
        int movesPlayed = 0;

        for (int i = 0; i < Parameters.GamesPerPairing; i++)
        {
            var gameState = new ConnectFourGameState();
            var currentAgent = agent1;
            var opponentAgent = agent2;

            while (gameState is { IsGameWon: false, IsGameDraw: false })
            {
                var move = currentAgent.GetNextAction(gameState);
                gameState.ExecuteMove(move);
                movesPlayed++;

                // Swap agents  
                (currentAgent, opponentAgent) = (opponentAgent, currentAgent);
            }

            // Evaluate the game result
            if (gameState.WinningPlayer == 1)
            {
                score += 1;
                gamesWon++;
            }
            else if (gameState.WinningPlayer == 2)
            {
                // Do nothing
            }
            else
            {
                score += 0.5; // Draw  
            }
        }

        return (score, gamesWon, movesPlayed / 2); // Divide by two because we tracked both players moves
    }
}
