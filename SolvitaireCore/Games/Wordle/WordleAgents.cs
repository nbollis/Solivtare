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
