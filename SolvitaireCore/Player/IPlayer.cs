namespace SolvitaireCore;

public interface IPlayer
{
    public IMove GetNextMove(GameState state);
}


public class Gene
{
    public readonly int LookAheadDepth;
}