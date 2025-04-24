namespace SolvitaireCore;

public interface IAgent
{
    string Name { get; }
    public IMove GetNextMove(IEnumerable<IMove> legalMoves);
}

public class RandomAgent : IAgent
{
    private Random _rng = new Random();
    public string Name => "Random Agent";


    public IMove GetNextMove(IEnumerable<IMove> legalMoves)
    {
        var moves = legalMoves.ToList();
        if (moves.Count == 0) return null;
        return moves[_rng.Next(moves.Count)];
    }
}