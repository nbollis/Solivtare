namespace SolvitaireCore;

public class AgentDecision
{
    public bool ShouldSkipGame { get; }
    public SolitaireMove? Move { get; }

    private AgentDecision(bool shouldSkipGame, SolitaireMove? move = null)
    {
        ShouldSkipGame = shouldSkipGame;
        Move = move;
    }

    public static AgentDecision PlayMove(SolitaireMove move) => new(false, move);
    public static AgentDecision SkipGame() => new(true);
}

