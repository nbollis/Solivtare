namespace SolvitaireCore;

public interface IAgent<in TGameState, out TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    string Name { get; }
    public TMove GetNextAction(TGameState gameState);
}