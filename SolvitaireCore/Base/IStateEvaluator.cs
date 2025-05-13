namespace SolvitaireCore;
public interface IStateEvaluator<TGameState> where TGameState : IGameState
{
    double Evaluate(TGameState state, int? moveCount = null);
}