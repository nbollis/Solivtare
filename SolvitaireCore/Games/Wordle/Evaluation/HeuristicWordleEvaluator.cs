namespace SolvitaireCore.Wordle;

/// <summary>
/// Heuristic-based Wordle evaluator that scores words based on:
/// - Letters in correct positions (highest priority)
/// - Known letters in different positions (medium priority)
/// - Unknown letters for information gain (medium priority)
/// - Avoidance of absent letters (penalty)
/// - Avoidance of wrong positions (penalty)
/// </summary>
public class HeuristicWordleEvaluator : WordleEvaluator
{
    // Tuned weights for different word features
    private const double CorrectPositionWeight = 100.0;
    private const double PresentLetterWeight = 50.0;
    private const double UnknownLetterWeight = 30.0;
    private const double AbsentLetterPenalty = -200.0;
    private const double WrongPositionPenalty = -150.0;
    private const double CommonLetterBonus = 5.0;

    // English letter frequency (approximate)
    private static readonly Dictionary<char, double> LetterFrequency = new()
    {
        {'E', 12.7}, {'T', 9.1}, {'A', 8.2}, {'O', 7.5}, {'I', 7.0},
        {'N', 6.7}, {'S', 6.3}, {'H', 6.1}, {'R', 6.0}, {'D', 4.3},
        {'L', 4.0}, {'C', 2.8}, {'U', 2.8}, {'M', 2.4}, {'W', 2.4},
        {'F', 2.2}, {'G', 2.0}, {'Y', 2.0}, {'P', 1.9}, {'B', 1.5},
        {'V', 1.0}, {'K', 0.8}, {'J', 0.15}, {'X', 0.15}, {'Q', 0.10}, {'Z', 0.07}
    };

    public HeuristicWordleEvaluator(string? firstWord = null)
    {
        FirstWord = firstWord?.ToUpperInvariant();
    }

    protected override double EvaluateWord(string word, WordleGameState state)
    {
        var knowledge = WordleKnowledge.FromGameState(state);
        double score = 0;

        // 1. HIGHEST PRIORITY: Letters in correct positions
        int correctPositions = CountCorrectPositions(word, knowledge);
        score += correctPositions * CorrectPositionWeight;

        // 2. HIGH PRIORITY: Known letters in word (but checking different positions)
        int presentLetters = CountPresentLettersInDifferentPosition(word, knowledge);
        score += presentLetters * PresentLetterWeight;

        // 3. MEDIUM PRIORITY: Unknown letters (information gain)
        int unknownLetters = CountUnknownLetters(word, knowledge);
        score += unknownLetters * UnknownLetterWeight;

        // 4. HEAVY PENALTY: Letters we know are NOT in the word
        int absentLetters = CountAbsentLetters(word, knowledge);
        score += absentLetters * AbsentLetterPenalty;

        // 5. HEAVY PENALTY: Letters in positions we know are wrong
        int wrongPositions = CountWrongPositions(word, knowledge);
        score += wrongPositions * WrongPositionPenalty;

        // 6. SMALL BONUS: Common letters (tie-breaker for early guesses)
        if (state.Guesses.Count < 2)
        {
            var uniqueLetters = CharHashSetPool.Get();
            try
            {
                foreach (char c in word)
                {
                    uniqueLetters.Add(c);
                }
                
                foreach (char c in uniqueLetters)
                {
                    if (LetterFrequency.TryGetValue(c, out double freq))
                    {
                        score += freq * CommonLetterBonus;
                    }
                }
            }
            finally
            {
                CharHashSetPool.Return(uniqueLetters);
            }
        }

        // 7. BONUS: Prefer words with unique letters early on (more information)
        if (state.Guesses.Count < 3)
        {
            var uniqueCount = word.Distinct().Count();
            if (uniqueCount == word.Length)
            {
                score += 20.0; // All unique letters
            }
        }

        return score;
    }

    public override IEnumerable<(WordleMove Move, double MoveScore)> OrderMoves(
        List<WordleMove> moves, 
        WordleGameState state, 
        bool bestFirst)
    {
        // Pre-build knowledge once
        var knowledge = WordleKnowledge.FromGameState(state);
        
        // Fast filter: eliminate words with known wrong features
        var candidateMoves = moves.Where(move =>
        {
            string word = move.Word;
            
            // Must not have absent letters
            if (CountAbsentLetters(word, knowledge) > 0)
                return false;
            
            // Must not have letters in wrong positions
            if (CountWrongPositions(word, knowledge) > 0)
                return false;
            
            // Must have all known correct letters in correct positions
            for (int i = 0; i < word.Length && i < knowledge.CorrectLetters.Length; i++)
            {
                if (knowledge.CorrectLetters[i].Count > 0)
                {
                    if (!knowledge.CorrectLetters[i].Contains(word[i]))
                        return false;
                }
            }
            
            // Must contain all known-present letters
            var wordLetters = CharHashSetPool.Get();
            try
            {
                foreach (char c in word)
                {
                    wordLetters.Add(c);
                }
                
                foreach (char knownLetter in knowledge.KnownInWord)
                {
                    if (!wordLetters.Contains(knownLetter))
                        return false;
                }
                
                return true;
            }
            finally
            {
                CharHashSetPool.Return(wordLetters);
            }
        }).ToList();

        // If filtering left us with nothing, fall back to all moves
        if (candidateMoves.Count == 0)
            candidateMoves = moves;

        // Score the candidates
        var scoredMoves = candidateMoves.Select(move =>
            (Move: move, MoveScore: EvaluateWord(move.Word, state)));

        return bestFirst
            ? scoredMoves.OrderByDescending(m => m.MoveScore)
            : scoredMoves.OrderBy(m => m.MoveScore);
    }
}
