namespace SolvitaireCore;

/// <summary>
/// Picks a move at random from the list of legal moves.
/// </summary>
public class RandomAgent<TGameState, TMove> : BaseAgent<TGameState, TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    private readonly Random _random = new Random();
    public override string Name => "Random Agent";

    public override TMove GetNextAction(TGameState gameState)
    {
        var moves = gameState.GetLegalMoves();

        var move = moves[_random.Next(moves.Count)];
        return move;
    }
}