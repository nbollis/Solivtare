using System.Reflection.Metadata.Ecma335;

namespace SolvitaireCore.Wordle;

/// <summary>
/// Base agent for playing Wordle
/// </summary>
public abstract class BaseWordleAgent : BaseAgent<WordleGameState, WordleMove>
{
    public override double EvaluateMoveWithAgent(WordleGameState gameState, WordleMove move, int? perspectivePlayer = null)
    {
        // For Wordle, we can evaluate moves based on information gain
        // For now, return 0 as baseline
        return 0.0;
    }
}

/// <summary>
/// Random Wordle agent - picks a random valid word
/// </summary>
public class RandomWordleAgent : BaseWordleAgent
{
    private readonly Random _random;
    public override string Name => "Random Wordle Agent";

    public RandomWordleAgent()
    {
        _random = new Random();
    }

    public override WordleMove GetNextAction(WordleGameState gameState, CancellationToken? cancellationToken = null)
    {
        var legalMoves = gameState.GetLegalMoves();
        
        if (legalMoves.Count == 0)
            return new WordleMove(gameState.TargetWord); // Shouldn't happen, but return target as fallback

        // Pick a random move
        var randomIndex = _random.Next(legalMoves.Count);
        return legalMoves[randomIndex];
    }
}

/// <summary>
/// Heuristic Wordle agent - uses smart heuristics to pick words
/// </summary>
public class HeuristicWordleAgent : BaseWordleAgent, ISearchAgent<WordleGameState, WordleMove>
{
    private readonly Random _random = new();
    
    public override string Name => string.IsNullOrEmpty(FirstWord) 
        ? "Heuristic Wordle Agent" 
        : $"Heuristic Wordle Agent ({FirstWord})";
    
    public int MaxDepth { get; set; } = 1; // Not really used for Wordle, but required by interface
    public StateEvaluator<WordleGameState, WordleMove> Evaluator { get; }
    
    /// <summary>
    /// Optional first word to always use as opening guess
    /// </summary>
    public string? FirstWord { get; set; }

    public HeuristicWordleAgent(string? firstWord = null)
    {
        FirstWord = firstWord?.ToUpperInvariant();
        Evaluator = new HeuristicWordleEvaluator(FirstWord);
    }

    public override WordleMove GetNextAction(WordleGameState gameState, CancellationToken? cancellationToken = null)
    {
        var allMoves = gameState.GetLegalMoves();
        if (gameState.Guesses.Count == 0 && FirstWord != "" && FirstWord != null)
        {
            var match = allMoves.First(p => p.Word == FirstWord);
            return match;
        }

        var orderedMoves = Evaluator.OrderMoves(gameState.GetLegalMoves(), gameState, bestFirst: true).ToList();
        
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
        return Evaluator.EvaluateMove(gameState, move);
    }
}
