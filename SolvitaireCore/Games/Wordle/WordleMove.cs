namespace SolvitaireCore.Wordle;

public class WordleMove : IMove<WordleGameState>, IEquatable<WordleMove>
{
    private static readonly Lazy<List<WordleMove>> _allMoves = new Lazy<List<WordleMove>>(InitializeAllMoves);

    /// <summary>
    /// Gets all possible Wordle moves (cached for performance)
    /// </summary>
    public static IReadOnlyList<WordleMove> AllMoves => _allMoves.Value;

    public string Word { get; }
    public bool IsTerminatingMove => false;

    public WordleMove(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            throw new ArgumentException("Word cannot be null or empty", nameof(word));
        
        Word = word.ToUpperInvariant();
    }

    /// <summary>
    /// Initializes all possible moves from the valid guess word list (called once)
    /// </summary>
    private static List<WordleMove> InitializeAllMoves()
    {
        var validWords = WordleWordList.GetAllValidGuesses();
        var moves = new List<WordleMove>(validWords.Count);
        
        foreach (var word in validWords)
        {
            moves.Add(new WordleMove(word));
        }
        
        return moves;
    }

    public bool IsValid(WordleGameState gameState)
    {
        // Must be correct length
        if (Word.Length != gameState.WordLength)
            return false;

        // Game must not be over
        if (gameState.IsGameWon || gameState.IsGameLost)
            return false;

        // Must be in the valid word list
        return WordleWordList.IsValidGuess(Word);
    }

    public override string ToString() => Word;

    public bool Equals(WordleMove? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Word == other.Word;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((WordleMove)obj);
    }

    public override int GetHashCode()
    {
        return Word.GetHashCode();
    }
}
