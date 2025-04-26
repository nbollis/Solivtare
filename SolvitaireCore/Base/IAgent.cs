namespace SolvitaireCore;

public interface IAgent<in TGameState>
{
    string Name { get; }
    public AgentDecision GetNextAction(TGameState gameState);
}