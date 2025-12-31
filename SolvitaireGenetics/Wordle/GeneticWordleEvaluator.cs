using SolvitaireCore;
using SolvitaireCore.Wordle;

namespace SolvitaireGenetics;

/// <summary>
/// Genetic-based Wordle evaluator that uses chromosome weights
/// to score word choices
/// </summary>
public class GeneticWordleEvaluator : WordleEvaluator
{
    private readonly WordleChromosome _chromosome;

    // English letter frequency (approximate) - same as heuristic
    private static readonly Dictionary<char, double> LetterFrequency = new()
    {
        {'E', 12.7}, {'T', 9.1}, {'A', 8.2}, {'O', 7.5}, {'I', 7.0},
        {'N', 6.7}, {'S', 6.3}, {'H', 6.1}, {'R', 6.0}, {'D', 4.3},
        {'L', 4.0}, {'C', 2.8}, {'U', 2.8}, {'M', 2.4}, {'W', 2.4},
        {'F', 2.2}, {'G', 2.0}, {'Y', 2.0}, {'P', 1.9}, {'B', 1.5},
        {'V', 1.0}, {'K', 0.8}, {'J', 0.15}, {'X', 0.15}, {'Q', 0.10}, {'Z', 0.07}
    };

    public GeneticWordleEvaluator(WordleChromosome chromosome)
    {
        _chromosome = chromosome;
        MaximumScore = 10000;
    }

    protected override double EvaluateWord(string word, WordleGameState state)
    {
        var knowledge = WordleKnowledge.FromGameState(state);
        double score = 0;

        // Determine game phase
        int guessNumber = state.Guesses.Count;
        bool isEarlyGame = guessNumber < 2;
        bool isMidGame = guessNumber >= 2 && guessNumber < 4;
        bool isLateGame = guessNumber >= 4;

        // 1. Letters in correct positions
        int correctPositions = CountCorrectPositions(word, knowledge);
        double correctWeight = _chromosome.GetWeight(WordleChromosome.CorrectPositionWeightName);
        if (isLateGame)
        {
            correctWeight *= _chromosome.GetWeight(WordleChromosome.LateGameCorrectBonusName);
        }
        score += correctPositions * correctWeight;

        // 2. Known letters in word (different positions)
        int presentLetters = CountPresentLettersInDifferentPosition(word, knowledge);
        double presentWeight = _chromosome.GetWeight(WordleChromosome.PresentLetterWeightName);
        if (isMidGame)
        {
            presentWeight *= _chromosome.GetWeight(WordleChromosome.MidGamePresentBonusName);
        }
        score += presentLetters * presentWeight;

        // 3. Unknown letters (information gain)
        int unknownLetters = CountUnknownLetters(word, knowledge);
        double unknownWeight = _chromosome.GetWeight(WordleChromosome.UnknownLetterWeightName);
        if (isEarlyGame)
        {
            unknownWeight *= _chromosome.GetWeight(WordleChromosome.EarlyGameUnknownBonusName);
        }
        score += unknownLetters * unknownWeight;

        // 4. Absent letters (penalty)
        int absentLetters = CountAbsentLetters(word, knowledge);
        score += absentLetters * _chromosome.GetWeight(WordleChromosome.AbsentLetterPenaltyName);

        // 5. Wrong positions (penalty)
        int wrongPositions = CountWrongPositions(word, knowledge);
        score += wrongPositions * _chromosome.GetWeight(WordleChromosome.WrongPositionPenaltyName);

        // 6. Common letters bonus (early game)
        if (isEarlyGame)
        {
            var uniqueLetters = CharHashSetPool.Get();
            try
            {
                foreach (char c in word)
                {
                    uniqueLetters.Add(c);
                }
                
                double commonBonus = _chromosome.GetWeight(WordleChromosome.CommonLetterBonusName);
                foreach (char c in uniqueLetters)
                {
                    if (LetterFrequency.TryGetValue(c, out double freq))
                    {
                        score += freq * commonBonus;
                    }
                }
            }
            finally
            {
                CharHashSetPool.Return(uniqueLetters);
            }
        }

        // 7. Unique letters bonus
        var uniqueCount = word.Distinct().Count();
        score += uniqueCount * _chromosome.GetWeight(WordleChromosome.UniqueLetterBonusName);
        
        if (uniqueCount == word.Length)
        {
            score += _chromosome.GetWeight(WordleChromosome.AllUniqueLettersBonusName);
        }

        // 8. Position-specific weights for correct letters
        for (int i = 0; i < word.Length && i < knowledge.CorrectLetters.Length; i++)
        {
            if (knowledge.CorrectLetters[i].Contains(word[i]))
            {
                string positionWeightName = i switch
                {
                    0 => WordleChromosome.Position0WeightName,
                    1 => WordleChromosome.Position1WeightName,
                    2 => WordleChromosome.Position2WeightName,
                    3 => WordleChromosome.Position3WeightName,
                    4 => WordleChromosome.Position4WeightName,
                    _ => null
                };
                
                if (positionWeightName != null)
                {
                    score += _chromosome.GetWeight(positionWeightName);
                }
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
        
        // Fast filter: eliminate invalid words
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
