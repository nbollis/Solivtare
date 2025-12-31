using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using SolvitaireCore.Wordle;

namespace SolvitaireGUI;

/// <summary>
/// ViewModel for displaying a single letter in a Wordle guess
/// </summary>
public class WordleLetterViewModel : BaseViewModel
{
    private char _letter;
    public char Letter
    {
        get => _letter;
        set
        {
            _letter = value;
            OnPropertyChanged(nameof(Letter));
        }
    }

    private LetterFeedback _feedback;
    public LetterFeedback Feedback
    {
        get => _feedback;
        set
        {
            _feedback = value;
            OnPropertyChanged(nameof(Feedback));
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(ForegroundColor));
        }
    }

    public Brush BackgroundColor => Feedback switch
    {
        LetterFeedback.Correct => new SolidColorBrush(Color.FromRgb(106, 170, 100)), // Green
        LetterFeedback.Present => new SolidColorBrush(Color.FromRgb(201, 180, 88)),  // Yellow
        LetterFeedback.Absent => new SolidColorBrush(Color.FromRgb(120, 124, 126)),  // Gray
        _ => new SolidColorBrush(Color.FromRgb(211, 214, 218))  // Light gray for empty/default
    };

    public Brush ForegroundColor => _letter == ' ' || _feedback == LetterFeedback.Absent && _letter == '\0'
        ? Brushes.Black
        : Brushes.White;

    public WordleLetterViewModel(char letter = ' ', LetterFeedback feedback = LetterFeedback.Absent)
    {
        Letter = letter;
        Feedback = feedback;
    }
}

/// <summary>
/// ViewModel for displaying a single guess row
/// </summary>
public class WordleGuessRowViewModel : BaseViewModel
{
    public ObservableCollection<WordleLetterViewModel> Letters { get; }
    public bool IsCurrentGuess { get; set; }

    public WordleGuessRowViewModel(int wordLength)
    {
        Letters = new ObservableCollection<WordleLetterViewModel>();
        for (int i = 0; i < wordLength; i++)
        {
            Letters.Add(new WordleLetterViewModel());
        }
    }

    public void SetGuess(GuessResult result)
    {
        for (int i = 0; i < result.Word.Length && i < Letters.Count; i++)
        {
            Letters[i].Letter = result.Word[i];
            Letters[i].Feedback = result.Feedback[i];
        }
    }

    public void SetCurrentInput(string input)
    {
        for (int i = 0; i < Letters.Count; i++)
        {
            if (i < input.Length)
            {
                Letters[i].Letter = input[i];
                Letters[i].Feedback = LetterFeedback.Absent; // Default for input
            }
            else
            {
                Letters[i].Letter = ' ';
                Letters[i].Feedback = LetterFeedback.Absent;
            }
        }
    }

    public void Clear()
    {
        foreach (var letter in Letters)
        {
            letter.Letter = ' ';
            letter.Feedback = LetterFeedback.Absent;
        }
    }
}

/// <summary>
/// Main ViewModel for Wordle game state
/// </summary>
public class WordleGameStateViewModel : GameStateViewModel<WordleGameState, WordleMove>
{
    public ObservableCollection<WordleGuessRowViewModel> GuessRows { get; }

    private string _currentInput = string.Empty;
    public string CurrentInput
    {
        get => _currentInput;
        set
        {
            _currentInput = value?.ToUpperInvariant() ?? string.Empty;
            OnPropertyChanged(nameof(CurrentInput));
            OnPropertyChanged(nameof(CanSubmitGuess));
            UpdateCurrentRow();
        }
    }

    public bool CanSubmitGuess => 
        !GameState.IsGameWon && 
        !GameState.IsGameLost && 
        CurrentInput.Length == GameState.WordLength &&
        WordleWordList.IsValidGuess(CurrentInput);

    public ICommand SubmitGuessCommand { get; }
    public ICommand ClearInputCommand { get; }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged(nameof(StatusMessage));
        }
    }

    public WordleGameStateViewModel(WordleGameState gameState, IGameController<WordleGameState, WordleMove>? controller = null) 
        : base(gameState, controller)
    {
        GuessRows = new ObservableCollection<WordleGuessRowViewModel>();
        
        // Create all guess rows
        for (int i = 0; i < gameState.MaxGuesses; i++)
        {
            GuessRows.Add(new WordleGuessRowViewModel(gameState.WordLength));
        }

        SubmitGuessCommand = new RelayCommand(SubmitGuess);
        ClearInputCommand = new RelayCommand(ClearInput);

        UpdateBoard();
    }

    private void SubmitGuess()
    {
        if (!CanSubmitGuess)
            return;

        var move = new WordleMove(CurrentInput);
        ApplyMove(move);
        CurrentInput = string.Empty;
    }

    private void ClearInput()
    {
        CurrentInput = string.Empty;
    }

    private void UpdateCurrentRow()
    {
        if (GameState.IsGameWon || GameState.IsGameLost)
            return;

        var currentRowIndex = GameState.Guesses.Count;
        if (currentRowIndex < GuessRows.Count)
        {
            GuessRows[currentRowIndex].SetCurrentInput(CurrentInput);
        }
    }

    public override void UpdateBoard()
    {
        // Update all guess rows
        for (int i = 0; i < GameState.Guesses.Count && i < GuessRows.Count; i++)
        {
            GuessRows[i].SetGuess(GameState.Guesses[i]);
            GuessRows[i].IsCurrentGuess = false;
        }

        // Clear remaining rows
        for (int i = GameState.Guesses.Count; i < GuessRows.Count; i++)
        {
            GuessRows[i].Clear();
            GuessRows[i].IsCurrentGuess = (i == GameState.Guesses.Count);
        }

        // Update status message
        if (GameState.IsGameWon)
        {
            StatusMessage = $"?? You won! The word was {GameState.TargetWord}";
        }
        else if (GameState.IsGameLost)
        {
            StatusMessage = $"?? Game over. The word was {GameState.TargetWord}";
        }
        else
        {
            StatusMessage = $"Guess {GameState.Guesses.Count + 1} of {GameState.MaxGuesses}";
        }

        OnPropertyChanged(nameof(IsGameWon));
        OnPropertyChanged(nameof(GameState));
        OnPropertyChanged(nameof(CanSubmitGuess));
    }

    public override void ApplyMove(WordleMove move)
    {
        if (GameState.IsGameWon || GameState.IsGameLost)
            return;

        GameState.ExecuteMove(move);
        UpdateBoard();
    }

    public override void UndoMove(WordleMove move)
    {
        GameState.UndoMove(move);
        UpdateBoard();
    }
}

public class WordleGameStateModel : WordleGameStateViewModel
{
    public static WordleGameStateModel Instance => new WordleGameStateModel();

    private WordleGameStateModel() : base(new WordleGameState())
    {
    }
}
