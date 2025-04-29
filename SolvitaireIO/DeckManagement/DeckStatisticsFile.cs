using System.Collections.Concurrent;
using SolvitaireCore;

namespace SolvitaireIO;

/// <summary>
/// Manages a file containing winnable decks and their associated statistics.
/// </summary>
public class DeckStatisticsFile : IDeckFile
{
    private readonly object _fileLock = new(); // Lock object for thread safety
    private readonly string _filePath;
    private readonly ConcurrentDictionary<int, DeckStatistics> _cache = new(); // Thread-safe in-memory cache
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
            return _cache.Values.Select(statistics => statistics.Deck).ToList();
        }
    }

    /// <summary>
    /// Reads all deck statistics from the cache.
    /// </summary>
    public List<DeckStatistics> ReadAllDeckStatistics()
    {
        return _cache.Values.ToList(); // Return a copy of the cache values
    }

    public void AddDeck(StandardDeck deck)
    {
        AddOrUpdateWinnableDeck(deck, 0, false);
    }

    public void AddDeck(DeckStatistics stats)
    {
        var key = stats.Deck.GetHashCode();
        _cache.AddOrUpdate(
            key,
            // If the deck does not exist, create a new entry
            _ => stats,
            // If the deck exists, update its statistics
            (_, existingDeck) =>
            {
                existingDeck.TimesWon += stats.TimesWon;
                existingDeck.TimesPlayed += stats.TimesPlayed;
                existingDeck.FewestMovesToWin = Math.Min(existingDeck.FewestMovesToWin, stats.FewestMovesToWin);
                existingDeck.MovesPerAttempt.AddRange(stats.MovesPerAttempt);
                existingDeck.MovesPerWin.AddRange(stats.MovesPerWin);
                return existingDeck;
            });
        _isCacheDirty = true;
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
        var key = deck.GetHashCode(); // Generate a unique key for the deck

        _cache.AddOrUpdate(
            key,
            // If the deck does not exist, create a new entry
            _ => new DeckStatistics
            {
                Deck = deck,
                TimesWon = isWin ? 1 : 0,
                TimesPlayed = 1,
                FewestMovesToWin = isWin ? movesThisAttempt : int.MaxValue,
                MovesPerAttempt = new List<int> { movesThisAttempt },
                MovesPerWin = isWin ? new List<int> { movesThisAttempt } : new List<int>()
            },
            // If the deck exists, update its statistics
            (_, existingDeck) =>
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
                return existingDeck;
            });

        _isCacheDirty = true; // Mark the cache as dirty
    }

    /// <summary>
    /// Flushes the in-memory cache to the file.
    /// </summary>
    public void Flush()
    {
        if (_isCacheDirty)
        {
            var json = DeckSerializer.SerializeDeckStatisticsList(_cache.Values.ToList());
            File.WriteAllText(_filePath, json);
            _isCacheDirty = false; // Reset the dirty flag
        }
    }

    /// <summary>
    /// Loads the file data into the in-memory cache.
    /// </summary>
    /// <summary>
    /// Loads the file data into the in-memory cache.
    /// </summary>
    private void LoadCache()
    {
        var json = File.ReadAllText(_filePath);
        var statisticsList = DeckSerializer.DeserializeDeckStatisticsList(json);

        _cache.Clear();
        foreach (var stats in statisticsList)
        {
            var key = stats.Deck.GetHashCode();
            _cache[key] = stats;
        }
    }
}