namespace SolvitaireCore.Wordle;

/// <summary>
/// Base evaluator for Wordle moves. Unlike Solitaire, Wordle evaluation is based on 
/// information theory - we don't look ahead, we evaluate based on what we know so far.
/// </summary>
public abstract class WordleEvaluator : StateEvaluator<WordleGameState, WordleMove>
{
    // Pooled HashSets for temporary allocations during evaluation
    protected static readonly HashSetPool<char> CharHashSetPool = new(initialCapacity: 26);

    /// <summary>
    /// Tracks what we know about each letter position
    /// </summary>
    protected class WordleKnowledge
    {
        // Letters we know are in the correct position
        public HashSet<char>[] CorrectLetters { get; }
        
        // Letters we know are in the word but not in this position
        public HashSet<char>[] PresentLetters { get; }
        
        // Letters we know are NOT in the word at all
        public HashSet<char> AbsentLetters { get; }
        
        // Letters we know are IN the word (somewhere)
        public HashSet<char> KnownInWord { get; }

        public WordleKnowledge(int wordLength)
        {
            CorrectLetters = new HashSet<char>[wordLength];
            PresentLetters = new HashSet<char>[wordLength];
            AbsentLetters = new HashSet<char>();
            KnownInWord = new HashSet<char>();
            
            for (int i = 0; i < wordLength; i++)
            {
                CorrectLetters[i] = new HashSet<char>();
                PresentLetters[i] = new HashSet<char>();
            }
        }

        /// <summary>
        /// Build knowledge from game state guesses
        /// </summary>
        public static WordleKnowledge FromGameState(WordleGameState state)
        {
            var knowledge = new WordleKnowledge(state.WordLength);
            
            foreach (var guess in state.Guesses)
            {
                for (int i = 0; i < guess.Word.Length; i++)
                {
                    char letter = guess.Word[i];
                    
                    switch (guess.Feedback[i])
                    {
                        case LetterFeedback.Correct:
                            knowledge.CorrectLetters[i].Add(letter);
                            knowledge.KnownInWord.Add(letter);
                            break;
                        
                        case LetterFeedback.Present:
                            knowledge.PresentLetters[i].Add(letter);
                            knowledge.KnownInWord.Add(letter);
                            break;
                        
                        case LetterFeedback.Absent:
                            // Only add to absent if it's not already known to be in the word
                            if (!knowledge.KnownInWord.Contains(letter))
                            {
                                knowledge.AbsentLetters.Add(letter);
                            }
                            break;
                    }
                }
            }
            
            return knowledge;
        }
    }

    public override double EvaluateMove(WordleGameState state, WordleMove move)
    {
        if (state.Guesses.Any(p => p.Word == move.Word))
            return double.MinValue; // Discourage repeated guesses

        // For Wordle, we evaluate the word directly without simulating the move
        // because we don't know the target word
        return EvaluateWord(move.Word, state);
    }

    /// <summary>
    /// Evaluate a word based on current game knowledge
    /// </summary>
    protected abstract double EvaluateWord(string word, WordleGameState state);

    public override double EvaluateState(WordleGameState state, int? moveCount = null)
    {
        // For Wordle, state evaluation is less meaningful than move evaluation
        // Return a simple score based on progress
        if (state.IsGameWon)
            return MaximumScore;
        if (state.IsGameLost)
            return -MaximumScore;
        
        // Score based on guesses remaining
        return (state.MaxGuesses - state.Guesses.Count) * 10.0;
    }

    /// <summary>
    /// Calculate how many new letters this word would reveal
    /// </summary>
    protected int CountUnknownLetters(string word, WordleKnowledge knowledge)
    {
        int count = 0;
        var seenInWord = CharHashSetPool.Get();
        
        try
        {
            foreach (char c in word)
            {
                if (seenInWord.Contains(c))
                    continue;
                    
                seenInWord.Add(c);
                
                if (!knowledge.KnownInWord.Contains(c) && !knowledge.AbsentLetters.Contains(c))
                {
                    count++;
                }
            }
            
            return count;
        }
        finally
        {
            CharHashSetPool.Return(seenInWord);
        }
    }

    /// <summary>
    /// Calculate how many letters are in correct positions
    /// </summary>
    protected int CountCorrectPositions(string word, WordleKnowledge knowledge)
    {
        int count = 0;
        for (int i = 0; i < word.Length && i < knowledge.CorrectLetters.Length; i++)
        {
            if (knowledge.CorrectLetters[i].Contains(word[i]))
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Calculate how many known-present letters are in different positions
    /// </summary>
    protected int CountPresentLettersInDifferentPosition(string word, WordleKnowledge knowledge)
    {
        int count = 0;
        var seenInWord = CharHashSetPool.Get();
        
        try
        {
            for (int i = 0; i < word.Length; i++)
            {
                char c = word[i];
                
                // Don't count if already correct at this position
                if (knowledge.CorrectLetters[i].Contains(c))
                    continue;
                
                // Don't count same letter twice
                if (seenInWord.Contains(c))
                    continue;
                    
                seenInWord.Add(c);
                
                // Check if this is a letter we know is in the word
                if (knowledge.KnownInWord.Contains(c))
                {
                    count++;
                }
            }
            
            return count;
        }
        finally
        {
            CharHashSetPool.Return(seenInWord);
        }
    }

    /// <summary>
    /// Count how many letters we know are NOT in the word
    /// </summary>
    protected int CountAbsentLetters(string word, WordleKnowledge knowledge)
    {
        int count = 0;
        var seen = CharHashSetPool.Get();
        
        try
        {
            foreach (char c in word)
            {
                if (!seen.Contains(c) && knowledge.AbsentLetters.Contains(c))
                {
                    count++;
                    seen.Add(c);
                }
            }
            
            return count;
        }
        finally
        {
            CharHashSetPool.Return(seen);
        }
    }

    /// <summary>
    /// Count letters in wrong known positions (we know letter is in word but not here)
    /// </summary>
    protected int CountWrongPositions(string word, WordleKnowledge knowledge)
    {
        int count = 0;
        for (int i = 0; i < word.Length && i < knowledge.PresentLetters.Length; i++)
        {
            if (knowledge.PresentLetters[i].Contains(word[i]))
            {
                count++;
            }
        }
        return count;
    }
}
