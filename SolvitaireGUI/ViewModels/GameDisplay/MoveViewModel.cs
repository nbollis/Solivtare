using SolvitaireCore;

namespace SolvitaireGUI;
public class MoveViewModel : BaseViewModel
{
    public IMove Move { get; }
    public string MoveString { get; set; }
    public double Evaluation { get; set; }


    public MoveViewModel(IMove move, double eval)
    {
        Move = move;
        MoveString = move.ToString();
        Evaluation = eval;
    }
}

public class AgentPanelModel : AgentPanelViewModel
{
    public static AgentPanelModel Instance => new AgentPanelModel();

    public AgentPanelModel() : base("", [], null) { }
}