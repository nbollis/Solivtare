namespace SolvitaireCore;

public abstract class SolitaireEvaluator : IStateEvaluator<SolitaireGameState>
{
    public abstract double Evaluate(SolitaireGameState state);
}