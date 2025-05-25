namespace SolvitaireCore;

public interface IAgent<in TGameState, out TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    string Name { get; }
    public TMove GetNextAction(TGameState gameState);
}

public interface ISearchAgent<in TGameState, out TMove> : IAgent<TGameState, TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove
{
    public int MaxDepth { get; set; }
}