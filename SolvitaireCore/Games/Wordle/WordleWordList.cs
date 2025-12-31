using System.Reflection;

namespace SolvitaireCore.Wordle;

/// <summary>
/// Manages the word lists for Wordle game using embedded resources
/// </summary>
public static class WordleWordList
{
    private static readonly HashSet<string> _answerWords; // All words that can be answers
    private static readonly HashSet<string> _validGuessWords; // All words that can be guessed
    private static readonly List<string> _answerWordsList;  // All words that can be answers
    private static readonly Random _random = new();

    static WordleWordList()
    {
        // Load answer words from embedded resource
        _answerWords = LoadWordsFromResource("SolvitaireCore.Resources.wodleWords.txt");
        _answerWordsList = _answerWords.ToList();

        // Load all valid guess words from embedded resource
        var allWords = LoadWordsFromResource("SolvitaireCore.Resources.allWords.txt");
        
        // Filter to only 5-letter words and combine with answer words
        _validGuessWords = new HashSet<string>(
            allWords.Where(w => w.Length == 5),
            StringComparer.OrdinalIgnoreCase
        );
        
        // Ensure all answer words are also valid guesses
        foreach (var word in _answerWords)
        {
            _validGuessWords.Add(word);
        }
    }

    /// <summary>
    /// Loads words from an embedded resource file
    /// </summary>
    private static HashSet<string> LoadWordsFromResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
                throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");

            using (var reader = new StreamReader(stream))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Split by spaces and add each word
                    var wordsInLine = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in wordsInLine)
                    {
                        var trimmed = word.Trim().ToUpperInvariant();
                        if (!string.IsNullOrEmpty(trimmed))
                        {
                            words.Add(trimmed);
                        }
                    }
                }
            }
        }

        return words;
    }

    /// <summary>
    /// Gets a random word from the answer pool
    /// </summary>
    public static string GetRandomAnswer()
    {
        return _answerWordsList[_random.Next(_answerWordsList.Count)];
    }

    /// <summary>
    /// Gets a specific word from the answer pool by index (for testing/seeding)
    /// </summary>
    public static string GetAnswerByIndex(int index)
    {
        if (index < 0 || index >= _answerWordsList.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return _answerWordsList[index];
    }

    /// <summary>
    /// Gets the total number of possible answer words
    /// </summary>
    public static int AnswerWordCount => _answerWordsList.Count;

    /// <summary>
    /// Checks if a word is a valid answer word
    /// </summary>
    public static bool IsValidAnswer(string word)
    {
        return _answerWords.Contains(word.ToUpperInvariant());
    }

    /// <summary>
    /// Checks if a word is a valid guess (includes both answers and additional valid words)
    /// </summary>
    public static bool IsValidGuess(string word)
    {
        return _validGuessWords.Contains(word.ToUpperInvariant());
    }

    /// <summary>
    /// Gets all valid answer words (for agent training)
    /// </summary>
    public static IReadOnlyList<string> GetAllAnswers()
    {
        return _answerWordsList.AsReadOnly();
    }

    /// <summary>
    /// Gets all valid guess words
    /// </summary>
    public static IReadOnlyCollection<string> GetAllValidGuesses()
    {
        return _validGuessWords;
    }

    /// <summary>
    /// Gets the count of valid guess words
    /// </summary>
    public static int ValidGuessCount => _validGuessWords.Count;
}
