namespace SolvitaireCore.Wordle;

/// <summary>
/// Represents the result of a Wordle guess with feedback for each letter
/// </summary>
public class GuessResult : IEquatable<GuessResult>
{
    public string Word { get; }
    public LetterFeedback[] Feedback { get; }
    public bool IsCorrect => Feedback.All(f => f == LetterFeedback.Correct);

    public GuessResult(string word, LetterFeedback[] feedback)
    {
        if (string.IsNullOrWhiteSpace(word))
            throw new ArgumentException("Word cannot be null or empty", nameof(word));
        
        if (feedback == null || feedback.Length != word.Length)
            throw new ArgumentException("Feedback array must match word length", nameof(feedback));

        Word = word.ToUpperInvariant();
        Feedback = feedback;
    }

    /// <summary>
    /// Creates a GuessResult by comparing a guess against a target word
    /// </summary>
    public static GuessResult Create(string guess, string target)
    {
        if (guess.Length != target.Length)
            throw new ArgumentException("Guess and target must be same length");

        guess = guess.ToUpperInvariant();
        target = target.ToUpperInvariant();

        var feedback = new LetterFeedback[guess.Length];
        var targetLetters = new Dictionary<char, int>();

        // Count letters in target
        foreach (char c in target)
        {
            if (!targetLetters.ContainsKey(c))
                targetLetters[c] = 0;
            targetLetters[c]++;
        }

        // First pass: mark correct positions
        for (int i = 0; i < guess.Length; i++)
        {
            if (guess[i] == target[i])
            {
                feedback[i] = LetterFeedback.Correct;
                targetLetters[guess[i]]--;
            }
        }

        // Second pass: mark present letters
        for (int i = 0; i < guess.Length; i++)
        {
            if (feedback[i] == LetterFeedback.Correct)
                continue;

            if (targetLetters.ContainsKey(guess[i]) && targetLetters[guess[i]] > 0)
            {
                feedback[i] = LetterFeedback.Present;
                targetLetters[guess[i]]--;
            }
            else
            {
                feedback[i] = LetterFeedback.Absent;
            }
        }

        return new GuessResult(guess, feedback);
    }

    public override string ToString()
    {
        var feedbackStr = string.Join("", Feedback.Select(f => f switch
        {
            LetterFeedback.Correct => "??",
            LetterFeedback.Present => "??",
            LetterFeedback.Absent => "?",
            _ => "?"
        }));
        return $"{Word} {feedbackStr}";
    }

    public bool Equals(GuessResult? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Word == other.Word && Feedback.SequenceEqual(other.Feedback);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((GuessResult)obj);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Word);
        foreach (var f in Feedback)
            hash.Add(f);
        return hash.ToHashCode();
    }
}
