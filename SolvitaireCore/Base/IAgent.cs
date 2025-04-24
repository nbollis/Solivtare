namespace SolvitaireCore;


public interface IAgent
{
    string Name { get; }
}

public interface IAgent<TMove, TGameState> : IAgent 
    where TMove : IMove
    where TGameState : IGameState<TMove>
{
    public TMove GetNextMove(TGameState gameState);
}