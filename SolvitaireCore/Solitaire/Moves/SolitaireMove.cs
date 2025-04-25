namespace SolvitaireCore;

public abstract class SolitaireMove(Pile fromPile, Pile toPile) : ISolitaireMove
{
    public Pile FromPile { get; } = fromPile;
    public Pile ToPile { get; } = toPile;

    public abstract bool IsValid();
    public abstract void Execute();
    public abstract void Undo();
}