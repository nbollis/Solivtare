namespace SolvitaireCore;

public interface ISolitaireMove : IMove
{
    Pile ToPile { get; }
    Pile FromPile { get; }
}