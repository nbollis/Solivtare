using SolvitaireCore;
using SolvitaireCore.Wordle;

namespace SolvitaireGenetics;

/// <summary>
/// Genetic Wordle agent that uses evolved chromosome weights
/// </summary>
public class WordleGeneticAgent : BaseWordleAgent, ISearchAgent<WordleGameState, WordleMove>, IGeneticAgent<WordleChromosome>
{
    private readonly GeneticWordleEvaluator _evaluator;
    private readonly Random _random = new();
    
    public override string Name { get; }
    public int MaxDepth { get; set; } = 1; // Not really used for Wordle
    public StateEvaluator<WordleGameState, WordleMove> Evaluator => _evaluator;
    public WordleChromosome Chromosome { get; init; }
    
    /// <summary>
    /// Optional first word to always use as opening guess
    /// </summary>
    public string? FirstWord { get; set; }

    public double Fitness
    {
        get => Chromosome.Fitness;
        set => Chromosome.Fitness = value;
    }

    public WordleGeneticAgent(WordleChromosome chromosome, string? name = null, string? firstWord = null, int maxDepth = 1)
    {
        Chromosome = chromosome;
        FirstWord = firstWord?.ToUpperInvariant();
        _evaluator = new GeneticWordleEvaluator(chromosome, FirstWord);
        MaxDepth = maxDepth;
        Name = name ?? (string.IsNullOrEmpty(FirstWord) 
            ? "Genetic Wordle Agent" 
            : $"Genetic Wordle Agent ({FirstWord})");
    }

    public override WordleMove GetNextAction(WordleGameState gameState, CancellationToken? cancellationToken = null)
    {
        var orderedMoves = _evaluator.OrderMoves(gameState.GetLegalMoves(), gameState, bestFirst: true).ToList();
        
        if (orderedMoves.Count == 0)
            return new WordleMove(gameState.TargetWord);

        // Get the best score
        double bestScore = orderedMoves[0].MoveScore;
        
        // Get all moves with the best score (handle ties)
        // Use Where instead of TakeWhile to catch all ties, not just consecutive ones
        var bestMoves = orderedMoves
            .Where(m => Math.Abs(m.MoveScore - bestScore) < 1e-6)
            .ToList();

        // Pick randomly among tied best moves
        if (bestMoves.Count == 1)
            return bestMoves[0].Move;
        
        return bestMoves[_random.Next(bestMoves.Count)].Move;
    }

    public override double EvaluateMoveWithAgent(WordleGameState gameState, WordleMove move, int? perspectivePlayer = null)
    {
        return _evaluator.EvaluateMove(gameState, move);
    }

    public IGeneticAgent<WordleChromosome> CrossOver(IGeneticAgent<WordleChromosome> other, double crossoverRate = 0.5)
        => new WordleGeneticAgent(Chromosome.CrossOver(other.Chromosome, crossoverRate));

    public IGeneticAgent<WordleChromosome> Mutate(double mutationRate)
        => new WordleGeneticAgent(Chromosome.Mutate<WordleChromosome>(mutationRate));

    public IGeneticAgent<WordleChromosome> Clone()
        => new WordleGeneticAgent(Chromosome.Clone<WordleChromosome>());
}
