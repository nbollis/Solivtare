namespace SolivtaireCore;

public interface IPlayer
{
    public IMove GetNextMove(GameState state);
}