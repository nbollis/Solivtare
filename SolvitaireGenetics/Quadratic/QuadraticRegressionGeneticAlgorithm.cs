namespace SolvitaireGenetics;

public class QuadraticRegressionGeneticAlgorithm : GeneticAlgorithm<QuadraticChromosome, QuadraticGeneticAlgorithmParameters>
{
    private int _samplingSize = 10000;
    public double[] CorrectLine { get; }

    public QuadraticRegressionGeneticAlgorithm(QuadraticGeneticAlgorithmParameters parameters, QuadraticChromosome? chromosomeTemplate = null) : base(parameters, chromosomeTemplate)
    {
        CorrectLine = new double[_samplingSize];

        for (int i = 0; i < _samplingSize; i++)
        {
            double x = -3 + i * 6.0 / _samplingSize-1; // Generate 10000 points from -3 to 3
            double y = parameters.CorrectA * x * x * x + parameters.CorrectB * x * x + parameters.CorrectC * x + parameters.CorrectIntercept;
            CorrectLine[i] = y;
        }
    }

    public override double EvaluateFitness(QuadraticChromosome chromosome)
    {
        double a = chromosome.Get(QuadraticChromosome.A);
        double b = chromosome.Get(QuadraticChromosome.B);
        double c = chromosome.Get(QuadraticChromosome.C);
        double yInt = chromosome.Get(QuadraticChromosome.YIntercept);

        double[] chromosomeValues = new double[_samplingSize];
        for (int i = 0; i < _samplingSize; i++)
        {
            double x = -3 + i * 6.0 / _samplingSize-1; // Generate 10000 points from -3 to 3  
            double y = a * x * x * x + b * x * x + c * x + yInt;
            chromosomeValues[i] = y;
        }

        var fitness = 
             (NormalizedRMSE(CorrectLine, chromosomeValues) + CubicCurveSimilarityScore(CorrectLine, chromosomeValues))
            / 2;

        Logger?.AccumulateAgentLog(CurrentGeneration, chromosome, fitness, 0, 0, 0);
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

