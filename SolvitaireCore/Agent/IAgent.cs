namespace SolvitaireCore;

public interface IAgent
{
    string Name { get; }
    public IMove GetNextMove(GameState state);
}

public class RandomAgent : IAgent
{
    private Random _rng = new Random();
    public string Name => "Random Agent";


    public IMove GetNextMove(GameState state)
    {
        var moves = state.GetLegalMoves().ToList();
        if (moves.Count == 0) return null;
        return moves[_rng.Next(moves.Count)];
    }
}