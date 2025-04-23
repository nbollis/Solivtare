namespace SolivtaireCore;

public abstract class SolitaireMove(Pile fromPile, Pile toPile) : IMove
{
    public Pile FromPile { get; } = fromPile;
    public Pile ToPile { get; } = toPile;

    public abstract bool IsValid(GameState state);
    public abstract void Execute(GameState state);
}