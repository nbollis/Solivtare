namespace SolvitaireCore;

/// <summary>
/// Represents a move made by an agent
/// </summary>
public interface IMove
{
    bool IsValid(IGameState state);
    void Execute(IGameState state);
}