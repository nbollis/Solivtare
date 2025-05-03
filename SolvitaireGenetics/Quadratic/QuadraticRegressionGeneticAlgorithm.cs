namespace SolvitaireGenetics;

public class QuadraticRegressionGeneticAlgorithm : GeneticAlgorithm<QuadraticChromosome, QuadraticGeneticAlgorithmParameters>
{
    private List<(double X, double Y)> CorrectLine { get; init; }
    public QuadraticRegressionGeneticAlgorithm(QuadraticGeneticAlgorithmParameters parameters) : base(parameters)
    {
        CorrectLine = [];
        for (int x = -1000; x < 1000; x++)
        {
            double y = parameters.CorrectA * x * x + parameters.CorrectB * x + parameters.CorrectC + parameters.CorrectIntercept;
            CorrectLine.Add((x, y));
        }
    }

    protected override double EvaluateFitness(QuadraticChromosome chromosome)
    {
        double a = chromosome.Get(QuadraticChromosome.A);
        double b = chromosome.Get(QuadraticChromosome.B);
        double c = chromosome.Get(QuadraticChromosome.C);
        double yInt = chromosome.Get(QuadraticChromosome.YIntercept);

        double error = 0.0;

        for (int x = -1000; x < 1000; x++)
        {
            double y = a * x * x + b * x + c + yInt;
            error += Math.Pow(y - CorrectLine[x].Y, 2); // Squared error
        }

        return -error; // maximize fitness → minimize error
    }
}