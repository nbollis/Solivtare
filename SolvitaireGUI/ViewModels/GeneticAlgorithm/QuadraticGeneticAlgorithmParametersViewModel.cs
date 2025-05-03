using SolvitaireGenetics;

namespace SolvitaireGUI;

public class QuadraticGeneticAlgorithmParametersViewModel(QuadraticGeneticAlgorithmParameters parameters) : GeneticAlgorithmParametersViewModel(parameters)
{
    public double CorrectA
    {
        get => ((QuadraticGeneticAlgorithmParameters)_parameters).CorrectA;
        set
        {
            ((QuadraticGeneticAlgorithmParameters)_parameters).CorrectA = value;
            OnPropertyChanged(nameof(CorrectA));
        }
    }

    public double CorrectB
    {
        get => ((QuadraticGeneticAlgorithmParameters)_parameters).CorrectB;
        set
        {
            ((QuadraticGeneticAlgorithmParameters)_parameters).CorrectB = value;
            OnPropertyChanged(nameof(CorrectB));
        }
    }

    public double CorrectC
    {
        get => ((QuadraticGeneticAlgorithmParameters)_parameters).CorrectC;
        set
        {
            ((QuadraticGeneticAlgorithmParameters)_parameters).CorrectC = value;
            OnPropertyChanged(nameof(CorrectC));
        }
    }

    public double CorrectIntercept
    {
        get => ((QuadraticGeneticAlgorithmParameters)_parameters).CorrectIntercept;
        set
        {
            ((QuadraticGeneticAlgorithmParameters)_parameters).CorrectIntercept = value;
            OnPropertyChanged(nameof(CorrectIntercept));
        }
    }
}

public class QuadraticGeneticAlgorithmParametersModel : QuadraticGeneticAlgorithmParametersViewModel
{
    public static QuadraticGeneticAlgorithmParametersModel Instance => new();

    QuadraticGeneticAlgorithmParametersModel() : base(new QuadraticGeneticAlgorithmParameters())
    {

    }
}