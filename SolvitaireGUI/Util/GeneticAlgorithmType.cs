using SolvitaireCore.ConnectFour;
using SolvitaireGenetics;

namespace SolvitaireGUI;

public enum GeneticAlgorithmType
{
    Solitaire,
    Quadratic,
    ConnectFour
}

public static class GeneticAlgorithmTypeExtensions
{
    public static GeneticAlgorithmParameters ToNewParams(this GeneticAlgorithmType type)
    {
        return type switch
        {
            GeneticAlgorithmType.Solitaire => new SolitaireGeneticAlgorithmParameters(),
            GeneticAlgorithmType.Quadratic => new QuadraticGeneticAlgorithmParameters(),
            GeneticAlgorithmType.ConnectFour => new ConnectFourGeneticAlgorithmParameters(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static ChromosomeViewModel ToNewChromosomeViewModel(this GeneticAlgorithmType type)
    {
        return type switch
        {
            GeneticAlgorithmType.Solitaire => new ChromosomeViewModel(SolitaireChromosome.BestSoFar()),
            GeneticAlgorithmType.Quadratic => new ChromosomeViewModel(new QuadraticChromosome(Random.Shared)),
            GeneticAlgorithmType.ConnectFour => new ChromosomeViewModel(ConnectFourChromosome.BestSoFar()),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static GeneticAlgorithmType FromParams(this GeneticAlgorithmParameters parameters)
    {
        return parameters switch
        {
            SolitaireGeneticAlgorithmParameters => GeneticAlgorithmType.Solitaire,
            QuadraticGeneticAlgorithmParameters => GeneticAlgorithmType.Quadratic,
            ConnectFourGeneticAlgorithmParameters => GeneticAlgorithmType.ConnectFour,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static GeneticAlgorithmParametersViewModel ToNewViewModel(this GeneticAlgorithmType type)
    {
        return type switch
        {
            GeneticAlgorithmType.Solitaire => new SolitaireGeneticAlgorithmParametersViewModel(new SolitaireGeneticAlgorithmParameters()),
            GeneticAlgorithmType.Quadratic => new QuadraticGeneticAlgorithmParametersViewModel(new QuadraticGeneticAlgorithmParameters()),
            GeneticAlgorithmType.ConnectFour => new ConnectFourGeneticAlgorithmParametersViewModel(new ConnectFourGeneticAlgorithmParameters()),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static GeneticAlgorithmParametersViewModel ToViewModel(this GeneticAlgorithmType type, GeneticAlgorithmParameters parameters)
    {
        return type switch
        {
            GeneticAlgorithmType.Solitaire => new SolitaireGeneticAlgorithmParametersViewModel((SolitaireGeneticAlgorithmParameters)parameters),
            GeneticAlgorithmType.Quadratic => new QuadraticGeneticAlgorithmParametersViewModel((QuadraticGeneticAlgorithmParameters)parameters),
            GeneticAlgorithmType.ConnectFour => new ConnectFourGeneticAlgorithmParametersViewModel((ConnectFourGeneticAlgorithmParameters)parameters),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static GeneticAlgorithmParametersViewModel ToViewModel(this GeneticAlgorithmParameters parameters)
    {
        return parameters switch
        {
            SolitaireGeneticAlgorithmParameters algorithmParameters => new SolitaireGeneticAlgorithmParametersViewModel(algorithmParameters),
            QuadraticGeneticAlgorithmParameters algorithmParameters => new QuadraticGeneticAlgorithmParametersViewModel(algorithmParameters),
            ConnectFourGeneticAlgorithmParameters algorithmParameters => new ConnectFourGeneticAlgorithmParametersViewModel(algorithmParameters),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
