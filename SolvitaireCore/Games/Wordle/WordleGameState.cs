namespace SolvitaireCore.Wordle;

public class WordleGameState : BaseGameState<WordleMove>, IEquatable<WordleGameState>
{
    public const int DefaultMaxGuesses = 6;
    public const int DefaultWordLength = 5;

    public int MaxGuesses { get; }
    public int WordLength { get; }
    public string TargetWord { get; private set; }
    public List<GuessResult> Guesses { get; private set; }

    public override bool IsGameWon => Guesses.Any(g => g.IsCorrect);
    public override bool IsGameLost => !IsGameWon && Guesses.Count >= MaxGuesses;

    public WordleGameState(int maxGuesses = DefaultMaxGuesses, int wordLength = DefaultWordLength, string? targetWord = null)
    {
        MaxGuesses = maxGuesses;
        WordLength = wordLength;
        TargetWord = targetWord?.ToUpperInvariant() ?? WordleWordList.GetRandomAnswer();
        Guesses = new List<GuessResult>();

        if (TargetWord.Length != WordLength)
            throw new ArgumentException($"Target word must be {WordLength} letters long", nameof(targetWord));

        if (!WordleWordList.IsValidAnswer(TargetWord))
            throw new ArgumentException("Target word must be a valid answer word", nameof(targetWord));
    }

    public WordleGameState() : this(DefaultMaxGuesses, DefaultWordLength, null) { }

    protected override void ResetInternal()
    {
        Guesses.Clear();
        // Keep the current target word when resetting (don't generate a new one)
        // Target word stays the same - only clear guesses
    }

    /// <summary>
    /// Resets the game with a specific target word (useful for testing)
    /// </summary>
    public void ResetWithTargetWord(string targetWord)
    {
        if (string.IsNullOrWhiteSpace(targetWord))
            throw new ArgumentException("Target word cannot be null or empty", nameof(targetWord));

        targetWord = targetWord.ToUpperInvariant();

        if (targetWord.Length != WordLength)
            throw new ArgumentException($"Target word must be {WordLength} letters long", nameof(targetWord));

        if (!WordleWordList.IsValidAnswer(targetWord))
            throw new ArgumentException("Target word must be a valid answer word", nameof(targetWord));

        Guesses.Clear();
        TargetWord = targetWord;
    }

    protected override List<WordleMove> GenerateLegalMoves()
    {
        if (IsGameWon || IsGameLost)
            return new List<WordleMove>();

        // Return the cached list of all valid moves (no allocation needed!)
        // Note: We return a new List to maintain the contract, but the moves themselves are reused
        return [.. WordleMove.AllMoves];
    }

    protected override void ExecuteMoveInternal(WordleMove move)
    {
        if (!move.IsValid(this))
            throw new InvalidOperationException("Invalid move");

        var result = GuessResult.Create(move.Word, TargetWord);
        Guesses.Add(result);
    }

    protected override void UndoMoveInternal(WordleMove move)
    {
        if (Guesses.Count == 0)
            throw new InvalidOperationException("No moves to undo");

        // Find and remove the matching guess
        var lastGuess = Guesses[^1];
        if (lastGuess.Word != move.Word)
            throw new InvalidOperationException("Move to undo does not match last guess");

        Guesses.RemoveAt(Guesses.Count - 1);
    }

    protected override WordleGameState CloneInternal()
    {
        var clone = new WordleGameState(MaxGuesses, WordLength, TargetWord)
        {
            Guesses = new List<GuessResult>(Guesses),
            MovesMade = this.MovesMade
        };
        return clone;
    }

    public bool Equals(WordleGameState? other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (TargetWord != other.TargetWord) return false;
        if (MaxGuesses != other.MaxGuesses) return false;
        if (WordLength != other.WordLength) return false;
        if (Guesses.Count != other.Guesses.Count) return false;

        for (int i = 0; i < Guesses.Count; i++)
        {
            if (!Guesses[i].Equals(other.Guesses[i]))
                return false;
        }

        return true;
    }

    protected override int GenerateHashCode()
    {
        var hash = new HashCode();
        hash.Add(TargetWord);
        hash.Add(MaxGuesses);
        hash.Add(WordLength);
        foreach (var guess in Guesses)
            hash.Add(guess);
        return hash.ToHashCode();
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Wordle Game - Target: {TargetWord}");
        sb.AppendLine($"Guesses: {Guesses.Count}/{MaxGuesses}");
        foreach (var guess in Guesses)
        {
            sb.AppendLine(guess.ToString());
        }
        if (IsGameWon)
            sb.AppendLine("🎉 You won!");
        else if (IsGameLost)
            sb.AppendLine($"😞 Game over. The word was {TargetWord}");
        return sb.ToString();
    }
}
