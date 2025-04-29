using System.Text.Json;
using SolvitaireCore;

namespace SolvitaireIO;

/// <summary>
/// Manages a file containing winnable decks and their associated statistics.
/// </summary>
public class DeckStatisticsFile : IDeckFile
{
    private readonly object _fileLock = new(); // Lock object for thread safety
    private readonly string _filePath;
    private readonly List<DeckStatistics> _cache = new(); // In-memory cache for deck statistics
    private bool _isCacheDirty = false; // Tracks whether the cache has unsaved changes

    /// <summary>
    /// Constructs a DeckStatisticsFile from a file path. If the file does not exist, it will be created.
    /// </summary>
    public DeckStatisticsFile(string filePath)
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

        // Load existing data into the cache
        LoadCache();
    }

    /// <summary>
    /// Reads all decks from the cache.
    /// </summary>
    public List<StandardDeck> ReadAllDecks()
    {
        lock (_fileLock)
        {
            return _cache.Select(statistics => statistics.Deck).ToList();
        }
    }

    /// <summary>
    /// Reads all deck statistics from the cache.
    /// </summary>
    public List<DeckStatistics> ReadAllDeckStatistics()
    {
        lock (_fileLock)
        {
            return [.._cache]; // Return a copy of the cache
        }
    }

    public void AddDeck(StandardDeck deck)
    {
        AddOrUpdateWinnableDeck(deck, 0, false);
    }

    public void AddDecks(IEnumerable<StandardDeck> newDecks)
    {
        foreach (var deck in newDecks)
        {
            AddDeck(deck);
        }
    }

    /// <summary>
    /// Will increment the deck if it exists in the file, otherwise creates a new entry. 
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="movesThisAttempt"></param>
    /// <param name="isWin"></param>
    public void AddOrUpdateWinnableDeck(StandardDeck deck, int movesThisAttempt, bool isWin)
    {
        lock (_fileLock)
        {
            var existingDeck = _cache.FirstOrDefault(d => d.Deck.Seed == deck.Seed && d.Deck.Shuffles == deck.Shuffles);

            if (existingDeck != null)
            {
                existingDeck.TimesWon += isWin ? 1 : 0;
                existingDeck.TimesPlayed += 1;
                existingDeck.FewestMovesToWin = isWin
                    ? Math.Min(existingDeck.FewestMovesToWin, movesThisAttempt)
                    : existingDeck.FewestMovesToWin;
                existingDeck.MovesPerAttempt.Add(movesThisAttempt);
                if (isWin)
                {
                    existingDeck.MovesPerWin.Add(movesThisAttempt);
                }
            }
            else
            {
                var newDeck = new DeckStatistics
                {
                    Deck = deck,
                    TimesWon = isWin ? 1 : 0,
                    TimesPlayed = 1,
                    FewestMovesToWin = isWin ? movesThisAttempt : int.MaxValue,
                    MovesPerAttempt = new List<int> { movesThisAttempt },
                    MovesPerWin = isWin ? new List<int> { movesThisAttempt } : new List<int>()
                };
                _cache.Add(newDeck);
            }

            _isCacheDirty = true; // Mark the cache as dirty
        }
    }

    /// <summary>
    /// Flushes the in-memory cache to the file.
    /// </summary>
    public void Flush()
    {
        lock (_fileLock)
        {
            if (_isCacheDirty)
            {
                var json = DeckSerializer.SerializeDeckStatisticsList(_cache);
                File.WriteAllText(_filePath, json);
                _isCacheDirty = false; // Reset the dirty flag
            }
        }
    }

    /// <summary>
    /// Loads the file data into the in-memory cache.
    /// </summary>
    private void LoadCache()
    {
        lock (_fileLock)
        {
            var json = File.ReadAllText(_filePath);
            _cache.Clear();
            _cache.AddRange(DeckSerializer.DeserializeDeckStatisticsList(json));
        }
    }
}