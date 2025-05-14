using MathNet.Numerics.Statistics;

namespace SolvitaireCore;

public static class ChromosomeOperations
{
    public static TChromosome GetAverageChromosome<TChromosome>(this List<TChromosome> chromosomes)
        where TChromosome : Chromosome
    {
        if (chromosomes == null || chromosomes.Count == 0)
            throw new ArgumentException("The list of chromosomes cannot be null or empty.", nameof(chromosomes));

        var firstChromosome = chromosomes[0].Clone<TChromosome>();
        foreach (var key in firstChromosome.MutableStatsByName.Keys.ToList())
        {
            double averageValue = chromosomes.Average(chromosome => chromosome.MutableStatsByName[key]);
            firstChromosome.MutableStatsByName[key] = averageValue;
        }

        firstChromosome.SpeciesIndex = -2;
        firstChromosome.Fitness = chromosomes.Select(p => p.Fitness).Average();
        return firstChromosome;
    }

    public static TChromosome GetStandardDeviationChromosome<TChromosome>(this List<TChromosome> chromosomes)
        where TChromosome : Chromosome
    {
        if (chromosomes == null || chromosomes.Count == 0)
            throw new ArgumentException("The list of chromosomes cannot be null or empty.", nameof(chromosomes));

        var firstChromosome = chromosomes[0].Clone<TChromosome>();
        foreach (var key in firstChromosome.MutableStatsByName.Keys.ToList())
        {
            double stdValue = chromosomes.Select(chromosome => chromosome.MutableStatsByName[key])
                .StandardDeviation();
            firstChromosome.MutableStatsByName[key] = stdValue;
        }

        firstChromosome.SpeciesIndex = -3;
        firstChromosome.Fitness = chromosomes.Select(p => p.Fitness).StandardDeviation();
        return firstChromosome;
    }

    /// <summary>
    /// Lower = more similar. Rooted for normalization. 
    /// </summary>
    /// <returns>The root of teh euclidean distance where lower is more similar</returns>
    public static double EuclideanDistance<TChromosome>(this TChromosome a, TChromosome b)
        where TChromosome : Chromosome
    {
        double sum = 0.0;
        foreach (var key in a.MutableStatsByName.Keys)
        {
            double diff = a.GetWeight(key) - b.GetWeight(key);
            sum += diff * diff;
        }
        return Math.Sqrt(sum);
    }

    /// <summary>
    /// Cosine similarity is a measure of similarity between two non-zero vectors of an inner product space.
    /// Useful if you care more about direction of weights than their absolute values
    /// </summary>
    /// <returns>A value from -1 to 1 where 1 is more similar. </returns>
    public static double CosineSimilarity<TChromosome>(this TChromosome a, TChromosome b)
        where TChromosome : Chromosome
    {
        double dot = 0, magA = 0, magB = 0;
        foreach (var key in a.MutableStatsByName.Keys)
        {
            double wa = a.GetWeight(key);
            double wb = b.GetWeight(key);
            dot += wa * wb;
            magA += wa * wa;
            magB += wb * wb;
        }

        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }

    /// <summary>
    /// Normalized Mean Absolute Error (MAE) is a measure of the average magnitude of the errors in a set of predictions, without considering their direction. Handy if you care about the absolute difference between weights.
    /// </summary>
    /// <returns>Scale invariant metric between 0 and 1 where 0 is more similar.</returns>
    public static double NormalizedMAE<TChromosome>(this TChromosome a, TChromosome b, double min = -3, double max = 3)
        where TChromosome : Chromosome
    {
        double sum = 0.0;
        int n = a.MutableStatsByName.Count;
        foreach (var key in a.MutableStatsByName.Keys)
        {
            sum += Math.Abs(a.GetWeight(key) - b.GetWeight(key));
        }
        return sum / (n * (max - min));
    }

    /// <summary>
    /// Measures the average pairwise distance between all chromosomes in the population.
    /// Tells how spread out your population is 
    /// </summary>
    /// <param name="population"></param>
    /// <returns></returns>
    public static double AveragePairwiseDiversity<TChromosome>(this List<TChromosome> population)
        where TChromosome : Chromosome
    {
        double total = 0.0;
        int count = 0;

        for (int i = 0; i < population.Count; i++)
        {
            for (int j = i + 1; j < population.Count; j++)
            {
                total += EuclideanDistance(population[i], population[j]);
                count++;
            }
        }

        return count > 0 ? total / count : 0.0;
    }

    /// <summary>
    /// Measures distance from the average of the population.
    /// Gives you an idea of the spread of the population.
    /// </summary>
    /// <param name="population"></param>
    /// <returns></returns>
    public static double VarianceFromCentroid<TChromosome>(this List<TChromosome> population)
        where TChromosome : Chromosome
    {
        var keys = population[0].MutableStatsByName.Keys;
        var centroid = new Dictionary<string, double>();

        foreach (var key in keys)
            centroid[key] = population.Average(chr => chr.GetWeight(key));

        double sumSquares = 0.0;
        foreach (var chr in population)
        {
            foreach (var key in keys)
            {
                double diff = chr.GetWeight(key) - centroid[key];
                sumSquares += diff * diff;
            }
        }

        return sumSquares / population.Count;
    }
}
