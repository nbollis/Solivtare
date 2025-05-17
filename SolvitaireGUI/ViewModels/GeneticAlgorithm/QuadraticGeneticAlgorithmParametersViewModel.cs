using SolvitaireGenetics;

namespace SolvitaireGUI;

public class QuadraticGeneticAlgorithmParametersViewModel(QuadraticGeneticAlgorithmParameters parameters) : GeneticAlgorithmParametersViewModel(parameters)
{
    public double CorrectA
    {
        get => ((QuadraticGeneticAlgorithmParameters)Parameters).CorrectA;
        set
        {
            ((QuadraticGeneticAlgorithmParameters)Parameters).CorrectA = value;
            OnPropertyChanged(nameof(CorrectA));
        }
    }

    public double CorrectB
    {
        get => ((QuadraticGeneticAlgorithmParameters)Parameters).CorrectB;
        set
        {
            ((QuadraticGeneticAlgorithmParameters)Parameters).CorrectB = value;
            OnPropertyChanged(nameof(CorrectB));
        }
    }

    public double CorrectC
    {
        get => ((QuadraticGeneticAlgorithmParameters)Parameters).CorrectC;
        set
        {
            ((QuadraticGeneticAlgorithmParameters)Parameters).CorrectC = value;
            OnPropertyChanged(nameof(CorrectC));
        }
    }

    public double CorrectIntercept
    {
        get => ((QuadraticGeneticAlgorithmParameters)Parameters).CorrectIntercept;
        set
        {
            ((QuadraticGeneticAlgorithmParameters)Parameters).CorrectIntercept = value;
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