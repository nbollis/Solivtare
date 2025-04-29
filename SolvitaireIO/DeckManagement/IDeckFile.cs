using SolvitaireCore;

namespace SolvitaireIO;

/// <summary>
/// Interface for managing a collection of decks.
/// </summary>
public interface IDeckFile
{
    /// <summary>
    /// Reads all decks from the file.
    /// </summary>
    List<StandardDeck> ReadAllDecks();

    /// <summary>
    /// Adds a new deck to the file.
    /// </summary>
    void AddDeck(StandardDeck deck);

    /// <summary>
    /// Adds multiple decks to the file.
    /// </summary>
    void AddDecks(IEnumerable<StandardDeck> newDecks);
}