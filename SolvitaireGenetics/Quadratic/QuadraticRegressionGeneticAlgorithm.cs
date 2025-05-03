namespace SolvitaireGenetics;

public class QuadraticRegressionGeneticAlgorithm : GeneticAlgorithm<QuadraticChromosome, QuadraticGeneticAlgorithmParameters>
{
    private List<(double X, double Y)> CorrectLine { get; }
    private double MaxError { get; }

    public QuadraticRegressionGeneticAlgorithm(QuadraticGeneticAlgorithmParameters parameters) : base(parameters)
    {
        CorrectLine = new List<(double X, double Y)>();
        MaxError = 0.0;

        for (int x = -1000; x < 1000; x++)
        {
            double y = parameters.CorrectA * x * x + parameters.CorrectB * x + parameters.CorrectC + parameters.CorrectIntercept;
            CorrectLine.Add((x, y));
            MaxError += Math.Pow(y, 2); // Precompute maximum possible error
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
            double correctY = CorrectLine[x + 1000].Y;
            error += Math.Pow(y - correctY, 2); // Squared error
        }

        double normalizedError = error / MaxError;
        return 1 - normalizedError; // Fitness ranges from -1 to 1
    }
}
