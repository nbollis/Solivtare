namespace SolvitaireCore;

public interface IPlayer
{
    public IMove GetNextMove(GameState state);
}