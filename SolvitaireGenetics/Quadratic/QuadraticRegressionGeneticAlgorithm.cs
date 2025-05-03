namespace SolvitaireGenetics;

public class QuadraticRegressionGeneticAlgorithm : GeneticAlgorithm<QuadraticChromosome, QuadraticGeneticAlgorithmParameters>
{
    public double[] CorrectLine { get; }

    public QuadraticRegressionGeneticAlgorithm(QuadraticGeneticAlgorithmParameters parameters, QuadraticChromosome? chromosomeTemplate = null) : base(parameters, chromosomeTemplate)
    {
        CorrectLine = new double[200];

        for (int x = -100; x < 100; x++)
        {
            double y = parameters.CorrectA * x * x * x + parameters.CorrectB * x * x + parameters.CorrectC * x + parameters.CorrectIntercept;
            CorrectLine[x + 100] = y;
        }
    }

    public override double EvaluateFitness(QuadraticChromosome chromosome)
    {
        double a = chromosome.Get(QuadraticChromosome.A);
        double b = chromosome.Get(QuadraticChromosome.B);
        double c = chromosome.Get(QuadraticChromosome.C);
        double yInt = chromosome.Get(QuadraticChromosome.YIntercept);

        double sumSquaredDifferences = 0.0;
        double sumCorrectSquared = 0.0;

        for (int x = -100; x < 100; x++)
        {
            double y = a * x * x * x + b * x * x + c * x + yInt;
            double correctY = CorrectLine[x + 100];
            sumSquaredDifferences += Math.Pow(y - correctY, 2);
            sumCorrectSquared += Math.Pow(correctY, 2);
        }

        double l2Norm = Math.Sqrt(sumSquaredDifferences) / Math.Sqrt(sumCorrectSquared);
        return 1 - l2Norm; // Fitness ranges from 0 to 1  
    }
}
