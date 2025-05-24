using System.Windows;
using System.Windows.Input;
using SolvitaireCore;
using SolvitaireIO;

namespace SolvitaireGUI;

public class GameInspectionTabViewModel : BaseViewModel
{
    private SolitaireGameStateViewModel _solitaireGameStateViewModel;
    private string _gameStateJson;
    private int _deckSeed;
    private int _deckShuffles;

    public GameInspectionTabViewModel()
    {
        var deck = new StandardDeck(23);
        deck.Shuffle();
        var gameState = new SolitaireGameState();
        gameState.DealCards(deck);
        SolitaireGameStateViewModel = new SolitaireGameStateViewModel(gameState);
        GameStateJson = GameStateSerializer.Serialize(gameState);

        DeckSeed = deck.Seed;
        DeckShuffles = deck.Shuffles;

        LoadFromJsonCommand = new RelayCommand(LoadFromJson);
        LoadDeckCommand = new RelayCommand(LoadDeck);
    }

    public SolitaireGameStateViewModel SolitaireGameStateViewModel
    {
        get => _solitaireGameStateViewModel;
        set
        {
            _solitaireGameStateViewModel = value;
            OnPropertyChanged(nameof(SolitaireGameStateViewModel));
        }
    }

    public string GameStateJson
    {
        get => _gameStateJson;
        set
        {
            _gameStateJson = value;
            OnPropertyChanged(nameof(GameStateJson));
        }
    }

    public int DeckSeed
    {
        get => _deckSeed;
        set
        {
            _deckSeed = value;
            OnPropertyChanged(nameof(DeckSeed));
        }
    }

    public int DeckShuffles
    {
        get => _deckShuffles;
        set
        {
            _deckShuffles = value;
            OnPropertyChanged(nameof(DeckShuffles));
        }
    }

    public ICommand LoadFromJsonCommand { get; set; }
    public ICommand LoadDeckCommand { get; set; }

    private void LoadFromJson()
    {
        try
        {
            var state = GameStateSerializer.Deserialize(GameStateJson);
            if (state == null)
            {
                MessageBox.Show("Invalid JSON format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SolitaireGameStateViewModel = new SolitaireGameStateViewModel(state);
        }
        catch (Exception e)
        {
            MessageBox.Show($"Error loading game state from JSON: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
    }

    private void LoadDeck()
    {
        try
        {
            var deck = new StandardDeck(DeckSeed);
            for (int i = 0; i < DeckShuffles; i++)
            {
                deck.Shuffle();
            }

            var gameState = new SolitaireGameState();
            gameState.DealCards(deck);
            SolitaireGameStateViewModel = new SolitaireGameStateViewModel(gameState);
        }
        catch (Exception e)
        {
            MessageBox.Show($"Error loading deck: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
    }
}

public class GameInspectionTabModel : GameInspectionTabViewModel
{
    public static GameInspectionTabModel Instance => new();

    GameInspectionTabModel()
    {

    }
}