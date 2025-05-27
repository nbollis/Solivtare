using SolvitaireGenetics;

namespace SolvitaireGUI;
public class MainWindowViewModel : BaseViewModel
{
    public GameInspectionTabViewModel GameInspectionTabViewModel { get; set; }
    public GeneticAlgorithmTabViewModel GeneticAlgorithmTabViewModel { get; set; }
    public GameHostViewModel GameHostViewModel { get; set; }

    public MainWindowViewModel()
    {
        //AgentPlayingViewModel = new AgentPlayingViewModel();
        GameInspectionTabViewModel = new();
        GameHostViewModel = new GameHostViewModel();
        GeneticAlgorithmTabViewModel = new(new SolitaireGeneticAlgorithmParameters());
    }
}

public class MainWindowModel : MainWindowViewModel
{
    public static MainWindowModel Instance => new();

    public MainWindowModel() : base()
    {

    }
}