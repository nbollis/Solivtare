using SolvitaireCore;

namespace SolvitaireIO;

/// <summary>
/// Represents a winnable deck and its associated statistics.
/// </summary>
public class DeckStatistics
{
    public StandardDeck Deck { get; set; }
    public int TimesWon { get; set; }
    public int TimesPlayed { get; set; }
    public int FewestMovesToWin { get; set; }
    public List<int> MovesPerAttempt { get; set; } = new();
    public List<int> MovesPerWin { get; set; } = new();
}

/// <summary>
/// DTO for DeckStatistics using MinimizedDeckDto for the deck.
/// </summary>
public class DeckStatisticsDto
{
    public MinimizedDeckDto Deck { get; set; }
    public int TimesWon { get; set; }
    public int TimesPlayed { get; set; }
    public int FewestMovesToWin { get; set; }
    public List<int> MovesPerAttempt { get; set; } = new();
    public List<int> MovesPerWin { get; set; } = new();
}