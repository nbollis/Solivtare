namespace SolvitaireCore;

public interface IMove
{
    /// <summary>
    /// This move terminates the game outside normal win/loss conditions. True for solitaire skip game. 
    /// </summary>
    bool IsTerminatingMove { get; }
}

/// <summary>
/// Represents a move made by an agent
/// </summary>
public interface IMove<in TGameState>: IMove where TGameState : IGameState
{
    bool IsValid(TGameState gameState);
}