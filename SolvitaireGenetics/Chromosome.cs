using MathNet.Numerics;
using MathNet.Numerics.Statistics;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace SolvitaireGenetics;

public abstract class Chromosome : IComparable<Chromosome>, IEquatable<Chromosome>
{
    private const int RoundingPlace = 2;
    protected readonly Random Random;
    protected bool CanFullRandomMutate = true;
    protected int WeightMinStartValue = -2;
    protected int WeightMaxStartValue = 2;

    public double Fitness { get; set; } = double.MinValue;
    public Dictionary<string, double> MutableStatsByName { get; set; }

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

    public TChromosome CrossOver<TChromosome>(TChromosome other, double crossoverRate = 0.5) 
        where TChromosome : Chromosome
    {
        return Crossover((TChromosome)this, other, crossoverRate);
    }

    public TChromosome Mutate<TChromosome>(double mutationRate)
        where TChromosome : Chromosome
    {
        return Mutate((TChromosome)this, mutationRate);
    }

    /// <summary>
    /// Creates a random weight from -2 to 2. 
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

    /// <summary>
    /// Mutates the chromosome by randomly changing its weights based on the mutation rate.
    /// Current mutation is to find a new double as defined by <see cref="GenerateRandomWeight"/>
    /// </summary>
    /// <param name="chromosome"></param>
    /// <param name="mutationRate"></param>
    /// <returns></returns>
    public static TChromosome Mutate<TChromosome>(TChromosome chromosome, double mutationRate) 
        where TChromosome : Chromosome
    {
        var newChromosome = chromosome.Clone<TChromosome>();

        foreach (var kvp in chromosome.MutableStatsByName)
        {
            // TODO: Get a better mutation system going. Maybe a gaussian distribution?
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

    public static TChromosome GetAverageChromosome<TChromosome>(List<TChromosome> chromosomes) 
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

        firstChromosome.Fitness = chromosomes.Select(p => p.Fitness).Average();
        return firstChromosome;
    }

    public static TChromosome GetStandardDeviationChromosome<TChromosome>(List<TChromosome> chromosomes) 
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

        firstChromosome.Fitness = chromosomes.Select(p => p.Fitness).StandardDeviation();
        return firstChromosome;
    }

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

    public string GetStableHash()
    {
        var ordered = MutableStatsByName.OrderBy(kvp => kvp.Key)
            .Select(kvp => $"{kvp.Key}:{kvp.Value:F6}"); // fixed precision
        var data = string.Join("|", ordered);

        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(bytes); // or Convert.ToBase64String
    }

    public int CompareTo(Chromosome? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Fitness.CompareTo(other.Fitness);
    }
}
