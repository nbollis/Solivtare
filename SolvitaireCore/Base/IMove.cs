namespace SolvitaireCore;

/// <summary>
/// Represents a move made by an agent
/// </summary>
public interface IMove
{
    bool IsValid();
    void Execute();
    void Undo();
}