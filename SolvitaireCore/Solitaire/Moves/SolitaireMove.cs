namespace SolvitaireCore;

public abstract class SolitaireMove(int fromPileIndex, int toPileIndex) 
    : IMove<SolitaireGameState>
{
    public int FromPileIndex { get; } = fromPileIndex;
    public int ToPileIndex { get; } = toPileIndex;

    public abstract bool IsValid(SolitaireGameState gameState);
}