namespace SolvitaireCore;

public class RandomAgent : SolitaireAgent
{
    private Random _rng = new Random();
    public override string Name => "Random Agent";


    public override SolitaireMove GetNextMove(SolitaireGameState gameState)
    {
        var moves = gameState.GetLegalMoves().ToList();
        if (moves.Count == 0) return null;
        return moves[_rng.Next(moves.Count)];
    }
}