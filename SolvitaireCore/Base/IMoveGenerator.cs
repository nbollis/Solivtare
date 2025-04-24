namespace SolvitaireCore;

public interface IMoveGenerator<TMove> where TMove : IMove
{
    IEnumerable<TMove> GenerateMoves(IGameState<TMove> state);
}