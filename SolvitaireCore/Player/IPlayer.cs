namespace SolvitaireCore;

public interface IPlayer
{
    string Name { get; }
    public IMove GetNextMove(GameState state);
}

public class RandomPlayer : IPlayer
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