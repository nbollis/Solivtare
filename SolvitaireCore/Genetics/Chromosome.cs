using System.Security.Cryptography;
using System.Text;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;

namespace SolvitaireCore;

public abstract class Chromosome : IComparable<Chromosome>, IEquatable<Chromosome>
{
    private const int RoundingPlace = 4;
    protected readonly Random Random;
    protected bool CanFullRandomMutate = true;
    protected int WeightMinStartValue = -2;
    protected int WeightMaxStartValue = 2;
    public int SpeciesIndex = -1;
    public double Fitness { get; set; } = double.MinValue;

    public Dictionary<string, double> MutableStatsByName { get; set; }
    public double GetWeight(string name) => MutableStatsByName.GetValueOrDefault(name, 0);
    public void SetWeight(string name, double value) => MutableStatsByName[name] = value;

    protected Chromosome(Random random)
    {
        Random = random;
        MutableStatsByName = new();
    }

    protected Chromosome()
    {
        Random = Random.Shared;
        MutableStatsByName = new();
    }

    public virtual TChromosome Clone<TChromosome>()
        where TChromosome : Chromosome
    {
        var clone = (TChromosome)Activator.CreateInstance(typeof(TChromosome), Random);
        foreach (var kvp in MutableStatsByName)
        {
            clone.MutableStatsByName[kvp.Key] = kvp.Value;
        }
        return clone;
    }



    /// <summary>
    /// Creates a random weight from Min to Max Value
    /// </summary>
    /// <returns></returns>
    protected double GenerateRandomWeight()
    {
        var range = Math.Abs(WeightMaxStartValue - WeightMinStartValue);
        var min = Math.Min(WeightMinStartValue, WeightMaxStartValue);
        return (Random.NextDouble() * range + min).Round(RoundingPlace);
    }

    public static TChromosome CreateRandom<TChromosome>(Random random)
    {
        return (TChromosome)Activator.CreateInstance(typeof(TChromosome), random)!;
    }

    #region Evolution

    public TChromosome CrossOver<TChromosome>(TChromosome other, double crossoverRate = 0.5)
        where TChromosome : Chromosome
    {
        return Crossover((TChromosome)this, other, crossoverRate);
    }

    /// <summary>
    /// Mutates the chromosome by randomly changing its weights based on the mutation rate.
    /// Current mutation is to find a new double as defined by <see cref="GenerateRandomWeight"/>
    /// </summary>
    /// <param name="mutationRate"></param>
    /// <returns></returns>
    public TChromosome Mutate<TChromosome>(double mutationRate)
        where TChromosome : Chromosome
    {
        return Mutate((TChromosome)this, mutationRate);
    }


    public static TChromosome Mutate<TChromosome>(TChromosome chromosome, double mutationRate) 
        where TChromosome : Chromosome
    {
        var newChromosome = chromosome.Clone<TChromosome>();

        foreach (var kvp in chromosome.MutableStatsByName)
        {
            // TODO: GetWeight a better mutation system going. Maybe a gaussian distribution?
            var mutationValue = chromosome.Random.NextDouble();
            if (mutationValue < mutationRate && chromosome.CanFullRandomMutate)
            {
                newChromosome.MutableStatsByName[kvp.Key] = chromosome.GenerateRandomWeight();
            }

            // double the change of normal mutations is a +- 10% mutation. 
            if (mutationValue < mutationRate * 2)
            {
                var oldValue = newChromosome.MutableStatsByName[kvp.Key];
                var change = oldValue * (chromosome.Random.NextDouble() * 0.2 - 0.1); // Random value between -10% and +10%  
                var newValue = oldValue + change;
                newChromosome.MutableStatsByName[kvp.Key] = newValue.Round(RoundingPlace);
            }
        }
        return newChromosome;
    }

    /// <summary>
    /// Crossover between two chromosomes. This means that the child will inherit some of the weights from both parents.
    /// </summary>
    /// <param name="parent1"></param>
    /// <param name="parent2"></param>
    /// <returns></returns>
    public static TChromosome Crossover<TChromosome>(TChromosome parent1, TChromosome parent2, double crossoverRate = 0.5)
        where TChromosome : Chromosome
    {
        var child = parent1.Clone<TChromosome>();
        foreach (var kvp in parent1.MutableStatsByName)
        {
            if (parent1.Random.NextDouble() < crossoverRate)
            {
                child.MutableStatsByName[kvp.Key] = parent2.MutableStatsByName[kvp.Key];
            }
        }
        return child;
    }
    
    #endregion

    #region Speciation

    public static (List<List<TChromosome>> Clusters, double TotalIntraClusterDistance)
    KMeans<TChromosome>(List<TChromosome> population, int k, int iterations = 10)
    where TChromosome : Chromosome
    {
        if (population == null || population.Count == 0)
            throw new ArgumentException("Population cannot be null or empty.", nameof(population));

        if (k <= 0)
            throw new ArgumentException("Number of clusters (k) must be greater than 0.", nameof(k));

        if (k > population.Count)
            throw new ArgumentException("Number of clusters (k) cannot exceed the population size.", nameof(k));

        var rnd = new Random();
        var centroids = population.OrderBy(_ => rnd.Next()).Take(k).ToList();
        List<List<TChromosome>> species = Enumerable.Range(0, k).Select(_ => new List<TChromosome>()).ToList();

        for (int iter = 0; iter < iterations; iter++)
        {
            // Reset species
            species = Enumerable.Range(0, k).Select(_ => new List<TChromosome>()).ToList();

            // Assign each chromosome to the nearest centroid
            foreach (var chr in population)
            {
                int bestIndex = 0;
                double bestDist = double.MaxValue;

                for (int i = 0; i < k; i++)
                {
                    double dist = chr.EuclideanDistance(centroids[i]);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestIndex = i;
                    }
                }

                species[bestIndex].Add(chr);
            }

            // Update centroids for non-empty clusters
            for (int i = 0; i < k; i++)
            {
                if (species[i].Count > 0) // Only update centroid if the cluster is not empty
                {
                    centroids[i] = species[i].GetAverageChromosome();
                }
            }
        }

        // Compute total intra-cluster distance
        double totalDistance = 0.0;
        for (int i = 0; i < k; i++)
        {
            if (species[i].Count > 0) // Skip empty clusters
            {
                var centroid = species[i].GetAverageChromosome();
                foreach (var chr in species[i])
                {
                    totalDistance += chr.EuclideanDistance(centroid);
                }
            }
        }

        return (species.Where(p => p.Count > 0).ToList(), totalDistance);
    }

    public static List<List<TChromosome>> KMeansSpeciationElbow<TChromosome>(List<TChromosome> population, int kMin = 2, int kMax = 10)
        where TChromosome : Chromosome
    {
        if (population == null || population.Count == 0)
            throw new ArgumentException("Population cannot be null or empty.", nameof(population));

        if (kMin <= 0 || kMax <= 0 || kMin > kMax)
            throw new ArgumentException("Invalid range for kMin and kMax.");

        kMax = Math.Min(kMax, population.Count); // Ensure kMax does not exceed population size

        var distances = new List<(int K, double TotalDistance)>();

        for (int k = kMin; k <= kMax; k++)
        {
            var (_, totalDist) = KMeans(population, k);
            distances.Add((k, totalDist));
        }

        // Use a simple heuristic: find the "knee" where relative improvement drops
        int bestK = distances[0].K;
        double bestImprovement = 0;

        for (int i = 1; i < distances.Count - 1; i++)
        {
            double prev = distances[i - 1].TotalDistance;
            double curr = distances[i].TotalDistance;
            double next = distances[i + 1].TotalDistance;

            double improvement = prev - curr;
            double nextImprovement = curr - next;

            if (nextImprovement < 0.5 * improvement) // Elbow heuristic
            {
                bestK = distances[i].K;
                break;
            }
        }

        var (finalSpecies, _) = KMeans(population, bestK);
        for (int i = 0; i < finalSpecies.Count; i++)
        {
            var species = finalSpecies[i];
            foreach (var chrom in species)
            {
                chrom.SpeciesIndex = i;
            }
        }
        return finalSpecies;
    }

    /// <summary>
    /// Measures how diverse the chromosomes are within a species.
    /// </summary>
    /// <param name="species"></param>
    /// <returns></returns>
    public static double IntraSpeciesDiversity<TChromosome>(List<List<TChromosome>> species)
        where TChromosome : Chromosome
    {
        double total = 0;
        int count = 0;

        foreach (var group in species)
        {
            for (int i = 0; i < group.Count; i++)
            {
                for (int j = i + 1; j < group.Count; j++)
                {
                    total += group[i].EuclideanDistance(group[j]);
                    count++;
                }
            }
        }

        return count > 0 ? total / count : 0;
    }

    /// <summary>
    /// Takes the average of each species and calculates the distance between them.
    /// </summary>
    /// <param name="species"></param>
    /// <returns></returns>
    public static double InterSpeciesDiversity<TChromosome>(List<List<TChromosome>> species)
        where TChromosome : Chromosome
    {
        var centroids = species.Select(p => p.GetAverageChromosome()).ToList();

        double total = 0;
        int count = 0;

        for (int i = 0; i < centroids.Count; i++)
        {
            for (int j = i + 1; j < centroids.Count; j++)
            {
                total += centroids[i].EuclideanDistance(centroids[j]);   
                    count++;
            }
        }

        return count > 0 ? total / count : 0;
    }

    #endregion

    public bool Equals(Chromosome? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Math.Abs(Fitness - other.Fitness) > 1e-5) // Use a small epsilon for floating-point comparison
            return false;

        if (MutableStatsByName.Count != other.MutableStatsByName.Count)
            return false;

        foreach (var kvp in MutableStatsByName)
        {
            if (!other.MutableStatsByName.TryGetValue(kvp.Key, out var otherValue) || Math.Abs(kvp.Value - otherValue) > 1e-5) // Use epsilon for dictionary values
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;

            // Order keys to ensure consistent hashing
            foreach (var kvp in MutableStatsByName.OrderBy(k => k.Key))
            {
                // Combine key and value hashes
                hash = hash * 31 + kvp.Key.GetHashCode();
                hash = hash * 31 + kvp.Value.GetHashCode();
            }
            return hash;
        }
    }

    private string? _cachedStableHash;

    public string GetStableHash()
    {
        if (_cachedStableHash != null)
            return _cachedStableHash;

        var ordered = MutableStatsByName.OrderBy(kvp => kvp.Key)
            .Select(kvp => $"{kvp.Key}:{kvp.Value:F6}"); // fixed precision
        var data = string.Join("|", ordered);

        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
        _cachedStableHash = Convert.ToHexString(bytes); // or Convert.ToBase64String
        return _cachedStableHash;
    }

    public int CompareTo(Chromosome? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Fitness.CompareTo(other.Fitness);
    }
}
