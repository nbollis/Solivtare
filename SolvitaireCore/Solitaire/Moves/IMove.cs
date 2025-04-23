namespace SolvitaireCore;

/// <summary>
/// Represents a move made by a agent
/// </summary>
public interface IMove
{
    bool IsValid(GameState state);
    void Execute(GameState state);
}