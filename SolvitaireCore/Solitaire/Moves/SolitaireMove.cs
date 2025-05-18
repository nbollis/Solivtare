namespace SolvitaireCore;

public abstract class SolitaireMove(int fromPileIndex, int toPileIndex, bool shouldSkip = false) 
    : IMove<SolitaireGameState>
{
    public bool ShouldSkip { get; } = shouldSkip;
    public int FromPileIndex { get; } = fromPileIndex;
    public int ToPileIndex { get; } = toPileIndex;

    public abstract bool IsValid(SolitaireGameState gameState);
}