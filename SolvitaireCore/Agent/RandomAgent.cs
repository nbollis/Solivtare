namespace SolvitaireCore;

public class RandomAgent : ISolitaireAgent
{
    private Random _rng = new Random();
    public string Name => "Random Agent";


    public ISolitaireMove GetNextMove(IEnumerable<ISolitaireMove> legalMoves)
    {
        var moves = legalMoves.ToList();
        if (moves.Count == 0) return null;
        return moves[_rng.Next(moves.Count)];
    }
}