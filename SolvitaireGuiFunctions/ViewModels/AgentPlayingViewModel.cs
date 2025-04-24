using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using SolvitaireCore;

namespace SolvitaireGuiFunctions;

public class AgentPlayingViewModel : BaseViewModel
{
    private StandardDeck _deck;
    private IAgent _agent;

    public IAgent Agent
    {
        get => _agent;
        set
        {
            _agent = value;
            OnPropertyChanged(nameof(Agent));
        }
    }

    public GameStateViewModel GameStateViewModel { get; set; }

    public ICommand ResetGameCommand { get; set; }
    public ICommand MakeMoveCommand { get; set; }
    public ICommand NewGameCommand { get; set; }

    public AgentPlayingViewModel()
    {
        _deck ??= new StandardDeck();
        _deck.Shuffle();

        var deck = _deck.Clone() as StandardDeck;
        var gameState = new GameState();
        gameState.DealCards(deck);
        GameStateViewModel = new GameStateViewModel(gameState);
        Agent = new RandomAgent();


        ResetGameCommand = new RelayCommand(ResetGame);
        MakeMoveCommand = new RelayCommand(MakeMove);
        NewGameCommand = new RelayCommand(NewGame);
    }

    private void ResetGame()
    {
        var deck = _deck.Clone() as StandardDeck;
        var gameState = new GameState();
        gameState.DealCards(deck!);
        GameStateViewModel.GameState = gameState;
    }

    private void MakeMove()
    {
        var move = Agent.GetNextMove(GameStateViewModel.GameState);
        GameStateViewModel.MakeMove(move);
    }

    private void NewGame()
    {
        _deck.Shuffle();
        Refresh();
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(GameStateViewModel));
        OnPropertyChanged(nameof(Agent));
    }
}

public class AgentPlayingModel : AgentPlayingViewModel
{
    public static AgentPlayingModel Instance => new();

    AgentPlayingModel()
    {
        Agent = new RandomAgent();
    }
}