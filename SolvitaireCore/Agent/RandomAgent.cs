namespace SolvitaireCore;

public class RandomAgent : ISolitaireAgent
{
    private Random _rng = new Random();
    public string Name => "Random Agent";


    public ISolitaireMove GetNextMove(SolitaireGameState gameState)
    {
        var moves = gameState.GetLegalMoves().ToList();
        if (moves.Count == 0) return null;
        return moves[_rng.Next(moves.Count)];
    }
}