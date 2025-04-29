using System.Text.Json;
using System.Text.Json.Serialization;
using SolvitaireCore;

namespace SolvitaireIO;

public class DeckFile
{
    private readonly object _fileLock = new(); // Lock object for thread safety
    private readonly string _filePath;

    /// <summary>
    /// Constructs a DeckFile from a file path. If the file does not exist, it will be created.
    /// </summary>
    public DeckFile(string filePath)
    {
        _filePath = filePath;

        // Ensure the file exists
        if (!File.Exists(_filePath))
        {
            lock (_fileLock)
            {
                if (!File.Exists(_filePath)) // Double-check inside the lock
                {
                    File.WriteAllText(_filePath, "[]"); // Initialize with an empty JSON array
                }
            }
        }
    }

    /// <summary>
    /// Reads all decks from the file.
    /// </summary>
    public List<StandardDeck> ReadAllDecks()
    {
        lock (_fileLock)
        {
            var json = File.ReadAllText(_filePath);
            return DeckSerializer.DeserializeStandardDecks(json);
        }
    }

    /// <summary>
    /// Adds a new deck to the file.
    /// </summary>
    public void AddDeck(StandardDeck deck)
    {
        lock (_fileLock)
        {
            // Read existing decks
            var decks = ReadAllDecks();

            // Add the new deck
            decks.Add(deck);

            // Serialize and overwrite the file
            var json = DeckSerializer.SerializeStandardDecks(decks);
            File.WriteAllText(_filePath, json);
        }
    }

    /// <summary>
    /// Adds multiple decks to the file.
    /// </summary>
    public void AddDecks(IEnumerable<StandardDeck> newDecks)
    {
        lock (_fileLock)
        {
            // Read existing decks
            var decks = ReadAllDecks();

            // Add the new decks
            decks.AddRange(newDecks);

            // Serialize and overwrite the file
            var json = DeckSerializer.SerializeStandardDecks(decks);
            File.WriteAllText(_filePath, json);
        }
    }
}