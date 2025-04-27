namespace SolvitaireCore;

public interface IMove
{

}

/// <summary>
/// Represents a move made by an agent
/// </summary>
public interface IMove<in TGameState>: IMove where TGameState : IGameState
{
    bool IsValid(TGameState gameState);
}