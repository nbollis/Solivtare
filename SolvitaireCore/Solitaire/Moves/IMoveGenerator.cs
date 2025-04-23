namespace SolvitaireCore;

public interface IMoveGenerator
{
    IEnumerable<IMove> GenerateMoves(GameState state);
}