namespace SolvitaireCore;

public abstract class SolitaireAgent : IAgent<ISolitaireMove>
{
    public abstract string Name { get; }
    public abstract ISolitaireMove GetNextMove(IEnumerable<ISolitaireMove> legalMoves);
}