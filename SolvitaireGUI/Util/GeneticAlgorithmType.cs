using SolvitaireGenetics;

namespace SolvitaireGUI;

public enum GeneticAlgorithmType
{
    Solitaire,
    Quadratic
}

public static class GeneticAlgorithmTypeExtensions
{
    public static GeneticAlgorithmParameters ToNewParams(this GeneticAlgorithmType type)
    {
        return type switch
        {
            GeneticAlgorithmType.Solitaire => new SolitaireGeneticAlgorithmParameters(),
            GeneticAlgorithmType.Quadratic => new QuadraticGeneticAlgorithmParameters(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static ChromosomeViewModel ToNewChromosomeViewModel(this GeneticAlgorithmType type)
    {
        return type switch
        {
            GeneticAlgorithmType.Solitaire => new ChromosomeViewModel(GeneticSolitaireAlgorithm.BestSoFar()),
            GeneticAlgorithmType.Quadratic => new ChromosomeViewModel(new QuadraticChromosome(Random.Shared)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static GeneticAlgorithmType FromParams(this GeneticAlgorithmParameters parameters)
    {
        return parameters switch
        {
            SolitaireGeneticAlgorithmParameters => GeneticAlgorithmType.Solitaire,
            QuadraticGeneticAlgorithmParameters => GeneticAlgorithmType.Quadratic,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
