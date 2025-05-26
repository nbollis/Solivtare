using SolvitaireGenetics;

namespace SolvitaireGUI;

public class ConnectFourGeneticAlgorithmParametersViewModel(ConnectFourGeneticAlgorithmParameters parameters)
    : GeneticAlgorithmParametersViewModel(parameters)
{
    public double RandomAgentRatio
    {
        get => ((ConnectFourGeneticAlgorithmParameters)Parameters).RandomAgentRatio;
        set
        {
            ((ConnectFourGeneticAlgorithmParameters)Parameters).RandomAgentRatio = value;
            OnPropertyChanged(nameof(RandomAgentRatio));
        }
    }

    public int GamesPerPairing
    {
        get => ((ConnectFourGeneticAlgorithmParameters)Parameters).GamesPerPairing;
        set
        {
            ((ConnectFourGeneticAlgorithmParameters)Parameters).GamesPerPairing = value;
            OnPropertyChanged(nameof(GamesPerPairing));
        }
    }
}

public class ConnectFourGeneticAlgorithmParametersModel : ConnectFourGeneticAlgorithmParametersViewModel
{
    public static ConnectFourGeneticAlgorithmParametersModel Instance => new();

    ConnectFourGeneticAlgorithmParametersModel() : base(new ConnectFourGeneticAlgorithmParameters())
    {

    }
}