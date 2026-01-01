using SolvitaireCore;
using SolvitaireCore.Wordle;
using SolvitaireIO.Database.Models;

namespace SolvitaireGenetics;

public class GeneticWordleAlgorithm : GeneticAlgorithm<WordleChromosome, WordleGeneticAlgorithmParameters, WordleGeneticAgent>
{
    private readonly List<string> _targetWords;
    private int _currentTargetWordIndex = 0;

    public override event Action<AgentLog>? AgentCompleted;

    public GeneticWordleAlgorithm(WordleGeneticAlgorithmParameters parameters)
        : base(parameters)
    {
        // Load first word pool from file if specified
        parameters.LoadFirstWordPool();
        
        // Prepare fixed target words for consistent evaluation
        parameters.PrepareTargetWords();
        _targetWords = parameters.FixedTargetWords ?? new List<string>();

        if (_targetWords.Count == 0)
        {
            var words = new List<string>();
            for (int i = 0; i < parameters.GamesPerAgent; i++)
            {
                var word = WordleWordList.GetRandomAnswer();
                words.Add(word);
            }
            _targetWords = words;
        }

        Console.WriteLine($"Wordle GA initialized with {parameters.FirstWordPool.Count} first words and {_targetWords.Count} target words");
    }

    public override WordleGeneticAgent CreateFromChromosome(WordleChromosome chromosome)
    {
        // Get first word from pool using chromosome's FirstWordIndex
        string firstWord = GetFirstWordFromChromosome(chromosome);
        
        return new WordleGeneticAgent(chromosome, null, firstWord, 1);
    }

    private string GetFirstWordFromChromosome(WordleChromosome chromosome)
    {
        int index = (int)Math.Round(chromosome.GetWeight(WordleChromosome.FirstWordIndexName));
        
        // Ensure index is within bounds
        index = Math.Clamp(index, 0, Parameters.FirstWordPool.Count - 1);
        
        return Parameters.FirstWordPool[index];
    }

    public override double EvaluateFitness(WordleGeneticAgent agent, CancellationToken? cancellationToken = null)
    {
        int gamesPlayed = 0;
        int gamesWon = 0;
        int totalGuesses = 0;
        double fitness = 0.0;

        // Play games against fixed target words
        for (int i = 0; i < Parameters.GamesPerAgent; i++)
        {
            // Cycle through target words
            string targetWord = _targetWords[_currentTargetWordIndex % _targetWords.Count];
            _currentTargetWordIndex++;

            // Create game state with this target word
            var gameState = new WordleGameState(Parameters.MaxGuesses, Parameters.WordLength, targetWord);
            agent.ResetState();

            int guessesInThisGame = 0;

            // Play until game is won or lost
            while (!gameState.IsGameWon && !gameState.IsGameLost)
            {
                // Check for cancellation
                if (cancellationToken?.IsCancellationRequested == true)
                {
                    break;
                }

                // Get agent's move
                var move = agent.GetNextAction(gameState, cancellationToken);
                
                // Apply move
                gameState.ExecuteMove(move);
                guessesInThisGame++;

                // Safety check to prevent infinite loops
                if (guessesInThisGame > Parameters.MaxGuesses + 5)
                {
                    Console.WriteLine($"⚠️ Game exceeded max guesses, breaking. Target: {targetWord}");
                    break;
                }
            }

            gamesPlayed++;
            totalGuesses += guessesInThisGame;

            // Calculate fitness for this game
            if (gameState.IsGameWon)
            {
                gamesWon++;
                
                // Base score: 1 point for winning
                double gameScore = 1.0;
                
                // Speed bonus: reward winning in fewer guesses
                // Max bonus if won in 1 guess, decreasing bonus for more guesses
                int guessesSaved = Parameters.MaxGuesses - guessesInThisGame;
                double speedBonus = guessesSaved * Parameters.SpeedBonus;
                gameScore += speedBonus;
                
                fitness += gameScore;
            }
            else if (gameState.IsGameLost)
            {
                // Penalty for losing
                fitness += Parameters.LossPenalty;
            }

            // Check for cancellation between games
            if (cancellationToken?.IsCancellationRequested == true)
            {
                break;
            }
        }

        // Normalize by number of games played
        if (gamesPlayed > 0)
        {
            double avgGuesses = gamesPlayed > 0 ? (double)totalGuesses / gamesPlayed : 0;
            
            // Log agent performance
            var agentLog = new AgentLog
            {
                Generation = CurrentGeneration,
                Fitness = fitness,
                GamesPlayed = gamesPlayed,
                GamesWon = gamesWon,
                MovesMade = totalGuesses,
                Chromosome = ChromosomeLog.FromChromosome(agent.Chromosome)
            };

            AgentCompleted?.Invoke(agentLog);

            // Console output for monitoring
            if (gamesWon > gamesPlayed * 0.8) // Only log high performers to reduce noise
            {
                string firstWord = GetFirstWordFromChromosome(agent.Chromosome);
                Console.WriteLine($"  Agent: {firstWord} - Won {gamesWon}/{gamesPlayed} ({100.0 * gamesWon / gamesPlayed:F1}%) - Avg guesses: {avgGuesses:F2} - Fitness: {fitness:F2}");
            }
        }

        agent.Chromosome.Fitness = fitness;
        return fitness;
    }

    /// <summary>
    /// Get statistics about first word usage in current population
    /// </summary>
    public Dictionary<string, int> GetFirstWordDistribution()
    {
        var distribution = new Dictionary<string, int>();
        
        foreach (var agent in Population)
        {
            string firstWord = GetFirstWordFromChromosome(agent.Chromosome);
            if (!distribution.ContainsKey(firstWord))
            {
                distribution[firstWord] = 0;
            }
            distribution[firstWord]++;
        }

        return distribution;
    }

    /// <summary>
    /// Get the best performing first word from current generation
    /// </summary>
    public (string Word, double AvgFitness, int Count) GetBestFirstWord()
    {
        var wordFitness = new Dictionary<string, List<double>>();
        
        foreach (var agent in Population)
        {
            string firstWord = GetFirstWordFromChromosome(agent.Chromosome);
            if (!wordFitness.ContainsKey(firstWord))
            {
                wordFitness[firstWord] = new List<double>();
            }
            wordFitness[firstWord].Add(agent.Fitness);
        }

        var best = wordFitness
            .OrderByDescending(kvp => kvp.Value.Average())
            .First();

        return (best.Key, best.Value.Average(), best.Value.Count);
    }
}
