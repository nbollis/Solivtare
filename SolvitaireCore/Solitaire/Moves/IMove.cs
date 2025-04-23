namespace SolvitaireCore;

/// <summary>
/// Represents a move made by a player
/// </summary>
public interface IMove
{
    bool IsValid(GameState state);
    void Execute(GameState state);
}