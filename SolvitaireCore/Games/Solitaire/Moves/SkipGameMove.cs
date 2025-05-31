namespace SolvitaireCore;

public class SkipGameMove() : SolitaireMove(-1, -1, true), IMove
{
    public override bool IsValid(SolitaireGameState gameState) => true;
    public override string ToString() => "Skip Game";
}