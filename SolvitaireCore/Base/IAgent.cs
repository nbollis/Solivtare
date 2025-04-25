namespace SolvitaireCore;


public interface IAgent
{
    string Name { get; }
}

public interface IAgent<out TMove, in TGameState> : IAgent 
{
    public TMove GetNextMove(TGameState gameState);
}