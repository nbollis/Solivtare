using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using SolvitaireCore;
using SolvitaireCore.ConnectFour;
using SolvitaireGenetics;
using SolvitaireIO.Database.Models;
using Color = System.Windows.Media.Color;

namespace SolvitaireGUI;

public enum PlayerType { Agent, Human }

public class AgentPanelViewModel<TGameState, TMove, TAgent> : BaseViewModel, IGenerationLogConsumer
    where TGameState : IGameState<TMove>
    where TMove : IMove
    where TAgent : class, IAgent<TGameState, TMove>
{
    private readonly IGameController<TGameState, TMove> _controller;
    private readonly int _playerNumber;

    public string PlayerLabel { get; }

    #region Agent Selection and Setup

    private PlayerType _playerType;
    public PlayerType PlayerType
    {
        get => _playerType;
        set { _playerType = value; OnPropertyChanged(nameof(PlayerType)); OnPropertyChanged(nameof(IsSearchAgent)); }
    }
    public ObservableCollection<TAgent> AvailableAgents { get; }

    private TAgent _selectedAgent;
    public TAgent SelectedAgent
    {
        get => _selectedAgent;
        set
        {
            // Null occurs when we remove an agent from the available list, but it was the one that was selected. 
            if (_selectedAgent == value || value == null) 
                return; // Avoid redundant updates  

            _selectedAgent = value;

            // Ensure Evaluator and MaxDepth are set before triggering property changes  
            if (_selectedAgent is ISearchAgent<TGameState, TMove> searchAgent)
            {
                Evaluator = searchAgent.Evaluator;
                MaxDepth = searchAgent.MaxDepth;
            }
            else
            {
                Evaluator = new AllEqualStateEvaluator<TGameState, TMove>();
                MaxDepth = null; // Reset MaxDepth for non-search agents  
            }

            // Handle genetic agent-specific logic  
            if (_selectedAgent != null && _selectedAgent.GetType().GetInterfaces()
                .Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == typeof(IGeneticAgent<>)))
            {
                var chromosomeProperty = _selectedAgent.GetType().GetProperty("Chromosome");
                if (chromosomeProperty != null)
                {
                    var chromosome = chromosomeProperty.GetValue(_selectedAgent) as Chromosome;
                    if (chromosome != null)
                    {
                        SelectedAgentChromosomeViewModel = new ChromosomeViewModel(chromosome);
                    }
                    else
                    {
                        SelectedAgentChromosomeViewModel = null; // Reset if chromosome is null  
                    }
                }
            }
            else
            {
                SelectedAgentChromosomeViewModel = null; // Reset for non-genetic agents  
            }

            // Trigger property change notifications after all updates  
            OnPropertyChanged(nameof(SelectedAgent));
            OnPropertyChanged(nameof(MaxDepth)); // Notify MaxDepth may have changed  
            OnPropertyChanged(nameof(IsSearchAgent));
            OnPropertyChanged(nameof(IsGeneticAgentSelected));
            OnPropertyChanged(nameof(SupportsFirstWord));
            
            // Load first word from agent if it has one
            if (SupportsFirstWord)
            {
                var firstWordProperty = _selectedAgent.GetType().GetProperty("FirstWord");
                if (firstWordProperty != null)
                {
                    _firstWord = firstWordProperty.GetValue(_selectedAgent) as string;
                    OnPropertyChanged(nameof(FirstWord));
                }
            }
        }
    }

    public bool IsSearchAgent => PlayerType == PlayerType.Agent && SelectedAgent is ISearchAgent<TGameState, TMove>;

    public bool IsGeneticAgentSelected => _selectedAgent.GetType().GetInterfaces()
        .Where(i => i.IsGenericType)
        .Any(i => i.GetGenericTypeDefinition() == typeof(IGeneticAgent<>));

    /// <summary>
    /// First word configuration for Wordle agents (if supported)
    /// </summary>
    private string? _firstWord;
    public string? FirstWord
    {
        get => _firstWord;
        set
        {
            if (_firstWord != value)
            {
                _firstWord = value?.ToUpperInvariant();
                OnPropertyChanged(nameof(FirstWord));
                UpdateAgentFirstWord();
            }
        }
    }

    /// <summary>
    /// Whether the selected agent supports first word configuration
    /// </summary>
    public bool SupportsFirstWord => SelectedAgent != null && 
        SelectedAgent.GetType().GetProperty("FirstWord") != null;

    private void UpdateAgentFirstWord()
    {
        if (SelectedAgent == null || !SupportsFirstWord)
            return;

        var firstWordProperty = SelectedAgent.GetType().GetProperty("FirstWord");
        if (firstWordProperty != null && firstWordProperty.CanWrite)
        {
            firstWordProperty.SetValue(SelectedAgent, FirstWord);
            
            // Also update the evaluator if the agent has one
            if (SelectedAgent is ISearchAgent<TGameState, TMove> searchAgent)
            {
                var evaluatorFirstWordProperty = searchAgent.Evaluator?.GetType().GetProperty("FirstWord");
                if (evaluatorFirstWordProperty != null && evaluatorFirstWordProperty.CanWrite)
                {
                    evaluatorFirstWordProperty.SetValue(searchAgent.Evaluator, FirstWord);
                }
            }
            
            RefreshLegalMoves();
        }
    }


    public int? MaxDepth
    {
        get => SelectedAgent is ISearchAgent<TGameState, TMove> searchAgent
            ? searchAgent.MaxDepth
            : null;
        set
        {
            if (SelectedAgent is ISearchAgent<TGameState, TMove> searchAgent && value.HasValue)
            {
                searchAgent.MaxDepth = value.Value;
                OnPropertyChanged(nameof(MaxDepth));
            }
            RefreshLegalMoves();
        }
    }

    #endregion

    public ICommand SwitchToHumanCommand { get; }
    public ICommand SwitchToAgentCommand { get; }
    public ICommand MakeMoveCommand { get; }
    public ICommand MakeSpecificMoveCommand { get; }
    public ICommand StartAgentCommand { get; }
    public ICommand StopAgentCommand { get; }

    public AgentPanelViewModel(
        string playerLabel,
        int playerNumber,
        ObservableCollection<TAgent> availableAgents,
        IGameController<TGameState, TMove> controller)
    {
        _playerNumber = playerNumber;
        _controller = controller;

        PlayerLabel = playerLabel;
        AvailableAgents = availableAgents;
        SelectedAgent = availableAgents.First();
        PlayerType = PlayerType.Agent; // Default to agent
        if (SelectedAgent is ISearchAgent<TGameState, TMove> searchAgent)
        {
            Evaluator = searchAgent.Evaluator;
            MaxDepth = searchAgent.MaxDepth;
        }
        else
        {
            Evaluator = new AllEqualStateEvaluator<TGameState, TMove>();
        }

        // If we are in a single player game, no need to set colors. Picker visibility will be bound to this. 
        if (controller.CurrentGameState is not ITwoPlayerGameState<TMove>)
        {
            SetPlayerColorCallback = null;
        }

        

        SwitchToHumanCommand = new RelayCommand(() => PlayerType = PlayerType.Human);
        SwitchToAgentCommand = new RelayCommand(() => PlayerType = PlayerType.Agent);
        MakeMoveCommand = new RelayCommand(MakeAgentMove);
        StartAgentCommand = new RelayCommand(StartAgent);
        StopAgentCommand = new RelayCommand(StopAgent);
        MakeSpecificMoveCommand = new RelayCommand(MakeSpecificMove);

        OnPropertyChanged(nameof(IsAgentRunning));
    }

    #region Agent Play Control

    // Delegate to check if it's this agent's turn and to apply a move
    internal CancellationTokenSource? AgentRunningCancellationTokenSource;

    public bool CanStartAgent => !IsAgentRunning;
    public bool CanStopAgent => IsAgentRunning;
    public bool IsMyTurn =>
        _controller.IsGameActive && _controller.CurrentPlayer == _playerNumber && PlayerType == PlayerType.Agent;

    private bool _isAgentRunning;
    public bool IsAgentRunning
    {
        get => _isAgentRunning;
        private set
        {
            if (_isAgentRunning != value)
            {
                _isAgentRunning = value;
                OnPropertyChanged(nameof(IsAgentRunning));
                OnPropertyChanged(nameof(CanStartAgent));
                OnPropertyChanged(nameof(CanStopAgent));
            }
        }
    }

    public async void StartAgent()
    {
        if (AgentRunningCancellationTokenSource != null)
            return;
        IsAgentRunning = true;
        AgentRunningCancellationTokenSource = new CancellationTokenSource();
        var token = AgentRunningCancellationTokenSource.Token;

        try
        {
            while (PlayerType == PlayerType.Agent && SelectedAgent != null &&
                   !token.IsCancellationRequested)
            {
                if (!_controller.IsGameActive)
                {
                    await Task.Delay(500, token);
                }
                else if (_controller.CurrentPlayer == _playerNumber)
                {
                    MakeAgentMove();
                }

                await Task.Delay(100, token);
            }
        }
        catch (TaskCanceledException)
        {
            // If canceled, Agent should not continue on next game. 
            IsAgentRunning = false;
            AgentRunningCancellationTokenSource = null;
        }
    }

    private void StopAgent()
    {
        AgentRunningCancellationTokenSource?.Cancel();
        AgentRunningCancellationTokenSource = null;
        IsAgentRunning = false;
    }

    private void MakeAgentMove()
    {
        _controller.ApplyAgentMove(_playerNumber);
        MovesMade = _controller.CurrentGameState.MovesMade;
        RefreshLegalMoves();
    }

    private void MakeSpecificMove()
    {
        if (SelectedMove != null && _controller.CurrentPlayer == _playerNumber)
        {
            _controller.ApplyMove(SelectedMove.Move);
            MovesMade = _controller.CurrentGameState.MovesMade;
            RefreshLegalMoves();
        }
    }

    #endregion

    #region Gui Display

    protected StateEvaluator<TGameState, TMove> Evaluator;
    private MoveViewModel<TMove>? _selectedMove;
    public MoveViewModel<TMove>? SelectedMove
    {
        get => _selectedMove;
        set
        {
            _selectedMove = value;
            OnPropertyChanged(nameof(SelectedMove));
        }
    }

    private double _evaluation;
    public double Evaluation
    {
        get => _evaluation;
        set
        {
            _evaluation = value;
            OnPropertyChanged(nameof(Evaluation));
        }
    }

    private int _movesMade;
    public int MovesMade
    {
        get => _movesMade;
        set
        {
            _movesMade = value;
            OnPropertyChanged(nameof(MovesMade));
        }
    }

    public ObservableCollection<MoveViewModel<TMove>> LegalMoves { get; } = new();
    
    private CancellationTokenSource? _refreshCancellationTokenSource;
    
    public async void RefreshLegalMoves()
    {
        // Cancel any ongoing refresh
        _refreshCancellationTokenSource?.Cancel();
        _refreshCancellationTokenSource = new CancellationTokenSource();
        var token = _refreshCancellationTokenSource.Token;

        LegalMoves.Clear();
        if (!_controller.IsGameActive)
            return;

        try
        {
            var state = (TGameState)_controller.CurrentGameState.Clone();
            var legalMoves = _controller.GetLegalMoves();

            // For games with very large move spaces (like Wordle with 12,000+ moves), 
            // limit the displayed moves to prevent UI freezing
            const int MaxDisplayedMoves = 100;
            var movesToDisplay = legalMoves.Count > MaxDisplayedMoves
                ? legalMoves.Take(MaxDisplayedMoves).ToList()
                : legalMoves;

            // Process moves asynchronously to avoid blocking UI thread
            var moves = await Task.Run(() =>
            {
                var result = new List<MoveViewModel<TMove>>();
                foreach (var move in movesToDisplay)
                {
                    if (token.IsCancellationRequested)
                        break;

                    double eval = SelectedAgent.EvaluateMoveWithAgent(state, move, _playerNumber);
                    result.Add(new MoveViewModel<TMove>(move, eval));
                }
                return result.OrderByDescending(p => p.Evaluation).ToList();
            }, token);

            if (token.IsCancellationRequested)
                return;

            // Add to UI on UI thread
            foreach (var moveVm in moves)
            {
                LegalMoves.Add(moveVm);
            }

            OnPropertyChanged(nameof(IsMyTurn));
        }
        catch (OperationCanceledException)
        {
            // Refresh was cancelled, this is expected
        }
    }
    public Action<int, Color>? SetPlayerColorCallback { get; set; }

    private Color _playerColor;
    public Color PlayerColor
    {
        get => _playerColor;
        set
        {
            if (_playerColor != value)
            {
                _playerColor = value;
                OnPropertyChanged(nameof(PlayerColor));
                SetPlayerColorCallback?.Invoke(_playerNumber, _playerColor);
            }
        }
    }

    #endregion

    #region Generation Log Control

    // Holds all loaded generations
    private List<GenerationLog>? _loadedGenerations;
    public bool IsGenerationLogLoaded => _loadedGenerations != null && _loadedGenerations.Count > 0;

    // For slider binding
    public int MinGeneration => IsGenerationLogLoaded ? _loadedGenerations!.Min(g => g.Generation) : 0;
    public int MaxGeneration => IsGenerationLogLoaded ? _loadedGenerations!.Max(g => g.Generation) : 0;

    private int _selectedGeneration;
    public int SelectedGeneration
    {
        get => _selectedGeneration;
        set
        {
            if (_selectedGeneration != value)
            {
                _selectedGeneration = value;
                OnPropertyChanged(nameof(SelectedGeneration));
                UpdateSpecialAgents();
            }
        }
    }

    private ChromosomeViewModel? _selectedAgentChromsoome;
    public ChromosomeViewModel? SelectedAgentChromosomeViewModel // Updated whenever a genetic agent is selected
    {
        get => _selectedAgentChromsoome;
        set
        {
            _selectedAgentChromsoome = value;
            OnPropertyChanged(nameof(SelectedAgentChromosomeViewModel));
        }
    }

    // Special agent placeholders
    private const string BestByGenName = "Best by Generation";
    private const string AvgByGenName = "Average by Generation";
    private TAgent? _bestByGenAgent;
    private TAgent? _avgByGenAgent;

    public void LoadGenerationLogs(IEnumerable<GenerationLog> logs)
    {
        _loadedGenerations = logs.OrderBy(g => g.Generation).ToList();
        _selectedGeneration = _loadedGenerations.First().Generation;
        OnPropertyChanged(nameof(IsGenerationLogLoaded));
        OnPropertyChanged(nameof(MinGeneration));
        OnPropertyChanged(nameof(MaxGeneration));
        OnPropertyChanged(nameof(SelectedGeneration));
        UpdateSpecialAgents();
    }

    private void UpdateSpecialAgents()
    {
        // Remove old special agents if present
        bool wasBestByGenSelected = _bestByGenAgent != null && SelectedAgent == _bestByGenAgent;
        bool wasAvgByGenSelected = _avgByGenAgent != null && SelectedAgent == _avgByGenAgent;
        if (_bestByGenAgent != null) AvailableAgents.Remove(_bestByGenAgent);
        if (_avgByGenAgent != null) AvailableAgents.Remove(_avgByGenAgent);

        if (!IsGenerationLogLoaded)
            return;

        var gen = _loadedGenerations!.FirstOrDefault(g => g.Generation == SelectedGeneration);
        if (gen == null) return;

        // Create agents from chromosomes  
        if (gen.BestChromosome != null)
        {
            var bestChromosome = gen.BestChromosome.Create();
            _bestByGenAgent = bestChromosome.ToGeneticAgent<TAgent>(BestByGenName);
            AvailableAgents.Add(_bestByGenAgent);
        }
        if (gen.AverageChromosome != null)
        {
            var avgChromosome = gen.AverageChromosome.Create();
            _avgByGenAgent = avgChromosome.ToGeneticAgent<TAgent>(AvgByGenName);
            AvailableAgents.Add(_avgByGenAgent);
        }

        // Re-select the replacement special agent if it was previously selected  
        if (wasBestByGenSelected && _bestByGenAgent != null)
        {
            SelectedAgent = _bestByGenAgent;
        }
        else if (wasAvgByGenSelected && _avgByGenAgent != null)
        {
            SelectedAgent = _avgByGenAgent;
        }

        OnPropertyChanged(nameof(AvailableAgents));
    }

    #endregion
}

public class AgentPanelModel : AgentPanelViewModel<ConnectFourGameState, ConnectFourMove, MinimaxAgent<ConnectFourGameState, ConnectFourMove>>
{
    public static AgentPanelModel Instance => new AgentPanelModel();

    public AgentPanelModel() : base("", 1, [], null!) { }
}