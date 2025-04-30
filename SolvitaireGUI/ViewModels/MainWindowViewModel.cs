
using SolvitaireCore;

namespace SolvitaireGUI;
public class MainWindowViewModel : BaseViewModel
{
    private AgentPlayingViewModel _agentPlayingViewModel;

    public AgentPlayingViewModel AgentPlayingViewModel
    {
        get => _agentPlayingViewModel;
        set 
        { 
            _agentPlayingViewModel = value;
            OnPropertyChanged(nameof(AgentPlayingViewModel));
            _agentPlayingViewModel.Refresh();
        }
    }

    public GameInspectionTabViewModel GameInspectionTabViewModel { get; set; }
    public GeneticAlgorithmViewModel GeneticAlgorithmTabViewModel { get; set; }

    public MainWindowViewModel()
    {
        AgentPlayingViewModel = new AgentPlayingViewModel();
        GameInspectionTabViewModel = new();
        GeneticAlgorithmTabViewModel = new();
    }
}

public class MainWindowModel : MainWindowViewModel
{
    public static MainWindowModel Instance => new();

    public MainWindowModel() : base()
    {

    }
}