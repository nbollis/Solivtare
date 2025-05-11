using SolvitaireIO.Database.Models;

namespace SolvitaireGenetics;

public class QuadraticRegressionGeneticAlgorithm : GeneticAlgorithm<QuadraticChromosome, QuadraticGeneticAlgorithmParameters>
{
    private int _samplingSize = 10000;
    public double[] CorrectLine { get; }
    public override event Action<AgentLog>? AgentCompleted;

    public QuadraticRegressionGeneticAlgorithm(QuadraticGeneticAlgorithmParameters parameters) : base(parameters)
    {
        CorrectLine = new double[_samplingSize];
        for (int i = 0; i < _samplingSize; i++)
        {
            double x;
            if (i < _samplingSize / 2)
            {
                // First half of points from -1 to 1  
                x = -1 + i * 2.0 / (_samplingSize / 2 - 1);
            }
            else
            {
                // Second half of points from -5 to -1 and 1 to 5  
                int adjustedIndex = i - _samplingSize / 2;
                x = adjustedIndex < (_samplingSize / 4)
                    ? -5 + adjustedIndex * 4.0 / (_samplingSize / 4 - 1)
                    : 1 + (adjustedIndex - _samplingSize / 4) * 4.0 / (_samplingSize / 4 - 1);
            }

            double y = parameters.CorrectA * x * x * x + parameters.CorrectB * x * x + parameters.CorrectC * x +
                       parameters.CorrectIntercept;
            CorrectLine[i] = y;
        }
    }

    public override double EvaluateFitness(QuadraticChromosome chromosome, CancellationToken? cancellationToken = null)
    {
        double a = chromosome.GetWeight(QuadraticChromosome.A);
        double b = chromosome.GetWeight(QuadraticChromosome.B);
        double c = chromosome.GetWeight(QuadraticChromosome.C);
        double yInt = chromosome.GetWeight(QuadraticChromosome.YIntercept);

        double[] chromosomeValues = new double[_samplingSize];

        for (int i = 0; i < _samplingSize; i++)
        {
            double x;
            if (i < _samplingSize / 2)
            {
                // First half of points from -1 to 1  
                x = -1 + i * 2.0 / (_samplingSize / 2 - 1);
            }
            else
            {
                // Second half of points from -5 to -1 and 1 to 5  
                int adjustedIndex = i - _samplingSize / 2;
                x = adjustedIndex < (_samplingSize / 4)
                    ? -5 + adjustedIndex * 4.0 / (_samplingSize / 4 - 1)
                    : 1 + (adjustedIndex - _samplingSize / 4) * 4.0 / (_samplingSize / 4 - 1);
            }

            double y = a * x * x * x + b * x * x + c * x + yInt;
            chromosomeValues[i] = y;
        }

        var fitness = 
             ((NormalizedRMSE(CorrectLine, chromosomeValues) + CubicCurveSimilarityScore(CorrectLine, chromosomeValues))
            / 2);

        fitness = Math.Pow(fitness, 2); // Square the fitness score

        var agentLog = new AgentLog()
        {
            Chromosome = ChromosomeLog.FromChromosome(chromosome), 
            Fitness = fitness, 
            Generation = CurrentGeneration
        };

        AgentCompleted?.Invoke(agentLog);
        return fitness;
    }

    public static double CubicCurveSimilarityScore(double[] actual, double[] predicted)
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("Input arrays must have the same length.");

        int n = actual.Length;
        double meanActual = actual.Average();
        double meanPredicted = predicted.Average();

        double covariance = 0.0, varActual = 0.0, varPredicted = 0.0;
        for (int i = 0; i < n; i++)
        {
            double da = actual[i] - meanActual;
            double dp = predicted[i] - meanPredicted;
            covariance += da * dp;
            varActual += da * da;
            varPredicted += dp * dp;
        }

        double correlation = (varActual == 0 || varPredicted == 0)
            ? 0
            : covariance / Math.Sqrt(varActual * varPredicted);

        double scaledCorrelation = (correlation + 1) / 2.0;
        return scaledCorrelation;
    }

    public static double NormalizedRMSE(double[] actual, double[] predicted, double maxExpectedNRMSE = 1.0)
    {
        if (actual.Length != predicted.Length)
            throw new ArgumentException("Input arrays must be the same length.");

        int n = actual.Length;
        double sumSquaredError = 0.0;
        double minActual = double.MaxValue;
        double maxActual = double.MinValue;

        for (int i = 0; i < n; i++)
        {
            double err = predicted[i] - actual[i];
            sumSquaredError += err * err;

            if (actual[i] < minActual) minActual = actual[i];
            if (actual[i] > maxActual) maxActual = actual[i];
        }

        double range = maxActual - minActual;
        double rmse = Math.Sqrt(sumSquaredError / n);
        double nrmse = range == 0 ? 0 : rmse / range;
        double scaledNrmse = Math.Max(0, 1 - nrmse / maxExpectedNRMSE); // Scale NRMSE to [0, 1]
        return scaledNrmse;
    }
}

