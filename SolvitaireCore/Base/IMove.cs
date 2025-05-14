namespace SolvitaireCore;

/// <summary>
/// Represents a move made by an agent
/// </summary>
public interface IMove<in TGameState>
{
    bool IsValid(TGameState gameState);
}