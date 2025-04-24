namespace SolvitaireCore;

public abstract class SolitaireMove(Pile fromPile, Pile toPile) : ISolitaireMove
{
    public Pile FromPile { get; } = fromPile;
    public Pile ToPile { get; } = toPile;

    public abstract bool IsValid(IGameState state);
    public abstract void Execute(IGameState state);
    public abstract void Undo(IGameState state);
}