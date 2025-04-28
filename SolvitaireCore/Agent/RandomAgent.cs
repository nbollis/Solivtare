namespace SolvitaireCore;

public class RandomAgent : SolitaireAgent
{
    private readonly Random _random = new Random();
    public override string Name => "Random Agent";


    public override AgentDecision GetNextAction(SolitaireGameState gameState)
    {
        var moves = gameState.GetLegalMoves();
        if (moves.Count == 0)
            return AgentDecision.SkipGame();

        var move = moves[_random.Next(moves.Count)];
        return AgentDecision.PlayMove(move);
    }
}