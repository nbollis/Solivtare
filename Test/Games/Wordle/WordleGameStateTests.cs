using SolvitaireCore.Wordle;

namespace Test.Games.Wordle;

[TestFixture]
public class WordleGameStateTests
{
    // Use common Wordle answer words that are definitely in wodleWords.txt
    private const string TestTargetWord = "WATER";
    private const string AlternateWord = "WORLD";


    public class Results(string condition, int gameCount)
    {
        public string Condition { get; init; } = condition;
        public int GamesPlayed { get; set; } = gameCount;
        public int GamesWon { get; set; } = 0;
        public int GamesLost { get; set; } = 0;
        public int[] MovesMade { get; set; } = Array.Empty<int>();

    }

    [Test]
    public void RunWords()
    {
        int games = WordleWordList.AnswerWords.Count;
        string[] startWords = ["OCEAN", "SPEAR", "AUDIO", "AISLE", "CRANE", "LIONS"];
        List<string> allTargetWords = WordleWordList.AnswerWords.ToList();
       
        List<HeuristicWordleAgent> agents = new();
        for (int i = 0; i < startWords.Length; i++)
            agents.Add(new HeuristicWordleAgent(startWords[i]));

        // Create output directory if it doesn't exist
        var outDir = @"C:\Users\nboll\Documents\WordleRuns\FirstRun";
        Directory.CreateDirectory(outDir);

        // Write detailed CSV with one row per agent/game combination
        var detailedPath = Path.Combine(outDir, "wordle_detailed_results2.csv");
        using (var detailedWriter = new StreamWriter(new FileStream(detailedPath, FileMode.Create)))
        {
            // Header row
            detailedWriter.WriteLine("AgentName,FirstWord,GameIndex,TargetWord,GuessCount,Won,Lost,Guess1,Guess2,Guess3,Guess4,Guess5,Guess6");

            foreach (var agent in agents)
                for (int gameIndex = 0; gameIndex < allTargetWords.Count; gameIndex++)
                {
                    string targetWord = allTargetWords[gameIndex];
                    var game = new WordleGameState(targetWord: targetWord);

                    // Play the game
                    while (!game.IsGameWon && !game.IsGameLost)
                    {
                        var action = agent.GetNextAction(game);
                        game.ExecuteMove(action);
                    }

                    // Extract guesses (pad with empty strings if fewer than 6)
                    var guesses = game.Guesses.Select(g => g.Word).ToList();
                    while (guesses.Count < 6)
                        guesses.Add("");

                    // Write row: AgentName, FirstWord, GameIndex, TargetWord, GuessCount, Won, Lost, Guess1-6
                    detailedWriter.WriteLine(
                        $"{agent.Name}," +
                        $"{agent.FirstWord ?? "None"}," +
                        $"{gameIndex}," +
                        $"{targetWord}," +
                        $"{game.MovesMade}," +
                        $"{(game.IsGameWon ? 1 : 0)}," +
                        $"{(game.IsGameLost ? 1 : 0)}," +
                        $"{string.Join(",", guesses)}"
                    );
                }
        }
    }



    [Test]
    public void GameState_Constructor_DefaultValues_ShouldInitializeCorrectly()
    {
        // Act
        var gameState = new WordleGameState();

        // Assert
        Assert.That(gameState.MaxGuesses, Is.EqualTo(WordleGameState.DefaultMaxGuesses));
        Assert.That(gameState.WordLength, Is.EqualTo(WordleGameState.DefaultWordLength));
        Assert.That(gameState.TargetWord, Is.Not.Null);
        Assert.That(gameState.TargetWord.Length, Is.EqualTo(WordleGameState.DefaultWordLength));
        Assert.That(gameState.Guesses, Is.Empty);
        Assert.That(gameState.IsGameWon, Is.False);
        Assert.That(gameState.IsGameLost, Is.False);
    }

    [Test]
    public void GameState_Constructor_WithTargetWord_ShouldUseProvidedWord()
    {
        // Act
        var gameState = new WordleGameState(targetWord: TestTargetWord);

        // Assert
        Assert.That(gameState.TargetWord, Is.EqualTo(TestTargetWord));
    }

    [Test]
    public void GameState_Constructor_InvalidTargetWord_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new WordleGameState(targetWord: "ZZZZZ")); // Not in word list
        Assert.Throws<ArgumentException>(() => new WordleGameState(targetWord: "TEST")); // Wrong length
        Assert.Throws<ArgumentException>(() => new WordleGameState(targetWord: "TESTING")); // Wrong length
    }

    [Test]
    public void GameState_ExecuteMove_ValidGuess_ShouldAddGuessResult()
    {
        // Arrange
        var gameState = new WordleGameState(targetWord: TestTargetWord);
        var move = new WordleMove("ABOUT");

        // Act
        gameState.ExecuteMove(move);

        // Assert
        Assert.That(gameState.Guesses.Count, Is.EqualTo(1));
        Assert.That(gameState.Guesses[0].Word, Is.EqualTo("ABOUT"));
        Assert.That(gameState.MovesMade, Is.EqualTo(1));
    }

    [Test]
    public void GameState_ExecuteMove_CorrectGuess_ShouldWinGame()
    {
        // Arrange
        var gameState = new WordleGameState(targetWord: TestTargetWord);
        var move = new WordleMove(TestTargetWord);

        // Act
        gameState.ExecuteMove(move);

        // Assert
        Assert.That(gameState.IsGameWon, Is.True);
        Assert.That(gameState.IsGameLost, Is.False);
        Assert.That(gameState.Guesses[0].IsCorrect, Is.True);
    }

    [Test]
    public void GameState_ExecuteMove_MaxGuessesReached_ShouldLoseGame()
    {
        // Arrange
        var gameState = new WordleGameState(targetWord: TestTargetWord);
        var wrongGuesses = new[] { "ABOUT", "ALIVE", "ANGEL", "ARENA", "BRICK", "CLOUD" };

        // Act
        foreach (var guess in wrongGuesses)
        {
            gameState.ExecuteMove(new WordleMove(guess));
        }

        // Assert
        Assert.That(gameState.IsGameWon, Is.False);
        Assert.That(gameState.IsGameLost, Is.True);
        Assert.That(gameState.Guesses.Count, Is.EqualTo(6));
    }

    [Test]
    public void GameState_UndoMove_ShouldRemoveLastGuess()
    {
        // Arrange
        var gameState = new WordleGameState(targetWord: TestTargetWord);
        var move = new WordleMove("ABOUT");
        gameState.ExecuteMove(move);

        // Act
        gameState.UndoMove(move);

        // Assert
        Assert.That(gameState.Guesses.Count, Is.EqualTo(0));
        Assert.That(gameState.MovesMade, Is.EqualTo(0));
    }

    [Test]
    public void GameState_UndoMove_WrongMove_ShouldThrowException()
    {
        // Arrange
        var gameState = new WordleGameState(targetWord: TestTargetWord);
        gameState.ExecuteMove(new WordleMove("ABOUT"));
        var wrongMove = new WordleMove("ANGEL");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => gameState.UndoMove(wrongMove));
    }

    [Test]
    public void GameState_Reset_ShouldClearGuesses()
    {
        // Arrange
        var gameState = new WordleGameState(targetWord: TestTargetWord);
        gameState.ExecuteMove(new WordleMove("ABOUT"));
        gameState.ExecuteMove(new WordleMove("ANGEL"));

        // Act
        gameState.Reset();

        // Assert
        Assert.That(gameState.Guesses.Count, Is.EqualTo(0));
        Assert.That(gameState.MovesMade, Is.EqualTo(0));
        Assert.That(gameState.IsGameWon, Is.False);
        Assert.That(gameState.IsGameLost, Is.False);
    }

    [Test]
    public void GameState_ResetWithTargetWord_ShouldSetNewTargetWord()
    {
        // Arrange
        var gameState = new WordleGameState(targetWord: TestTargetWord);
        gameState.ExecuteMove(new WordleMove("ABOUT"));

        // Act
        gameState.ResetWithTargetWord(AlternateWord);

        // Assert
        Assert.That(gameState.TargetWord, Is.EqualTo(AlternateWord));
        Assert.That(gameState.Guesses.Count, Is.EqualTo(0));
    }

    [Test]
    public void GameState_Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        var original = new WordleGameState(targetWord: TestTargetWord);
        original.ExecuteMove(new WordleMove("ABOUT"));
        original.ExecuteMove(new WordleMove("ANGEL"));

        // Act
        var clone = (WordleGameState)original.Clone();

        // Assert
        Assert.That(clone.TargetWord, Is.EqualTo(original.TargetWord));
        Assert.That(clone.Guesses.Count, Is.EqualTo(original.Guesses.Count));
        Assert.That(clone.MovesMade, Is.EqualTo(original.MovesMade));
        Assert.That(clone.Equals(original), Is.True);
        
        // Ensure it's a deep copy
        clone.ExecuteMove(new WordleMove("ARENA"));
        Assert.That(clone.Guesses.Count, Is.Not.EqualTo(original.Guesses.Count));
    }

    [Test]
    public void GameState_Equals_IdenticalStates_ShouldReturnTrue()
    {
        // Arrange
        var state1 = new WordleGameState(targetWord: TestTargetWord);
        var state2 = new WordleGameState(targetWord: TestTargetWord);
        state1.ExecuteMove(new WordleMove("ABOUT"));
        state2.ExecuteMove(new WordleMove("ABOUT"));

        // Act & Assert
        Assert.That(state1.Equals(state2), Is.True);
    }

    [Test]
    public void GameState_Equals_DifferentStates_ShouldReturnFalse()
    {
        // Arrange
        var state1 = new WordleGameState(targetWord: TestTargetWord);
        var state2 = new WordleGameState(targetWord: AlternateWord);

        // Act & Assert
        Assert.That(state1.Equals(state2), Is.False);
    }

    [Test]
    public void GameState_GetLegalMoves_ActiveGame_ShouldReturnAllValidWords()
    {
        // Arrange
        var gameState = new WordleGameState(targetWord: TestTargetWord);

        // Act
        var legalMoves = gameState.GetLegalMoves();

        // Assert
        Assert.That(legalMoves.Count, Is.GreaterThan(0));
        Assert.That(legalMoves.All(m => m.IsValid(gameState)), Is.True);
    }

    [Test]
    public void GameState_GetLegalMoves_GameWon_ShouldReturnEmpty()
    {
        // Arrange
        var gameState = new WordleGameState(targetWord: TestTargetWord);
        gameState.ExecuteMove(new WordleMove(TestTargetWord));

        // Act
        var legalMoves = gameState.GetLegalMoves();

        // Assert
        Assert.That(legalMoves, Is.Empty);
    }

    [Test]
    public void GameState_GetLegalMoves_GameLost_ShouldReturnEmpty()
    {
        // Arrange
        var gameState = new WordleGameState(targetWord: TestTargetWord);
        var wrongGuesses = new[] { "ABOUT", "ALIVE", "ANGEL", "ARENA", "BRICK", "CLOUD" };
        foreach (var guess in wrongGuesses)
        {
            gameState.ExecuteMove(new WordleMove(guess));
        }

        // Act
        var legalMoves = gameState.GetLegalMoves();

        // Assert
        Assert.That(legalMoves, Is.Empty);
    }

    [Test]
    public void GameState_HashCode_IdenticalStates_ShouldHaveSameHash()
    {
        // Arrange
        var state1 = new WordleGameState(targetWord: TestTargetWord);
        var state2 = new WordleGameState(targetWord: TestTargetWord);
        state1.ExecuteMove(new WordleMove("ABOUT"));
        state2.ExecuteMove(new WordleMove("ABOUT"));

        // Act & Assert
        Assert.That(state1.GetHashCode(), Is.EqualTo(state2.GetHashCode()));
    }

    [Test]
    public void GuessResult_Create_CorrectWord_ShouldAllBeCorrect()
    {
        // Act
        var result = GuessResult.Create(TestTargetWord, TestTargetWord);

        // Assert
        Assert.That(result.IsCorrect, Is.True);
        Assert.That(result.Feedback.All(f => f == LetterFeedback.Correct), Is.True);
    }

    [Test]
    public void GuessResult_Create_NoMatchingLetters_ShouldAllBeAbsent()
    {
        // Act - BRICK vs WATER: no common letters
        var result = GuessResult.Create("BRICK", "WATER");

        // Assert
        Assert.That(result.IsCorrect, Is.False);
        Assert.That(result.Feedback[0], Is.EqualTo(LetterFeedback.Absent)); // B
        Assert.That(result.Feedback[1], Is.EqualTo(LetterFeedback.Present)); // R present in WATER
        Assert.That(result.Feedback[2], Is.EqualTo(LetterFeedback.Absent)); // I
        Assert.That(result.Feedback[3], Is.EqualTo(LetterFeedback.Absent)); // C
        Assert.That(result.Feedback[4], Is.EqualTo(LetterFeedback.Absent)); // K
    }

    [Test]
    public void GuessResult_Create_MixedFeedback_ShouldBeCorrect()
    {
        // Act - SMART vs WATER
        // Position:  0  1  2  3  4
        // Guess:     S  M  A  R  T
        // Target:    W  A  T  E  R
        var result = GuessResult.Create("SMART", "WATER");

        // Assert
        Assert.That(result.Feedback[0], Is.EqualTo(LetterFeedback.Absent));   // S not in target
        Assert.That(result.Feedback[1], Is.EqualTo(LetterFeedback.Absent));   // M not in target
        Assert.That(result.Feedback[2], Is.EqualTo(LetterFeedback.Present));  // A present but wrong position
        Assert.That(result.Feedback[3], Is.EqualTo(LetterFeedback.Present));  // R present but wrong position
        Assert.That(result.Feedback[4], Is.EqualTo(LetterFeedback.Present));  // T present but wrong position
    }

    [Test]
    public void GuessResult_Create_DuplicateLetters_ShouldHandleCorrectly()
    {
        // Act - SPEED vs WATER (E appears twice in SPEED, once in WATER at position 3)
        // Position:  0  1  2  3  4
        // Guess:     S  P  E  E  D
        // Target:    W  A  T  E  R
        // First pass marks position 3 E as Correct (uses up the only E in target)
        // Second pass finds position 2 E has no remaining E in target, so Absent
        var result = GuessResult.Create("SPEED", "WATER");

        // Assert
        Assert.That(result.Feedback[0], Is.EqualTo(LetterFeedback.Absent));   // S not in target
        Assert.That(result.Feedback[1], Is.EqualTo(LetterFeedback.Absent));   // P not in target
        Assert.That(result.Feedback[2], Is.EqualTo(LetterFeedback.Absent));   // E - the E at position 3 used up the only E in target
        Assert.That(result.Feedback[3], Is.EqualTo(LetterFeedback.Correct));  // E correct at position 3
        Assert.That(result.Feedback[4], Is.EqualTo(LetterFeedback.Absent));   // D not in target
    }

    [Test]
    public void WordList_LoadsFromEmbeddedResources_ShouldHaveWords()
    {
        // Assert - verify words were loaded from resources
        Assert.That(WordleWordList.AnswerWordCount, Is.GreaterThan(1500)); // wodleWords.txt has ~1656 words
        Assert.That(WordleWordList.ValidGuessCount, Is.GreaterThan(10000)); // allWords.txt filtered to 5-letter words
    }

    [Test]
    public void WordleMove_AllMoves_ShouldBeCached()
    {
        // Act
        var moves1 = WordleMove.AllMoves;
        var moves2 = WordleMove.AllMoves;

        // Assert - same instance (cached)
        Assert.That(ReferenceEquals(moves1, moves2), Is.True);
        Assert.That(moves1.Count, Is.GreaterThan(10000));
    }
}
