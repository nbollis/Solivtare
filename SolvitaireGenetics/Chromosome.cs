using MathNet.Numerics;
using MathNet.Numerics.Statistics;

namespace SolvitaireGenetics;

public abstract class Chromosome<TChromosome> where TChromosome : Chromosome<TChromosome>
{
    private const int RoundingPlace = 3;
    protected readonly Random Random;
    public double Fitness { get; set; } = double.NegativeInfinity;
    public Dictionary<string, double> MutableStatsByName { get; set; }

    protected Chromosome(Random random)
    {
        Random = random;
        MutableStatsByName = new();
    }

    public TChromosome CrossOver(TChromosome other, double crossoverRate = 0.5) => Crossover(this, other, crossoverRate);

    public TChromosome Mutate(double mutationRate) => Mutate(this, mutationRate);

    public abstract TChromosome Clone();

    /// <summary>
    /// Creates a random weight from -2 to 2. 
    /// </summary>
    /// <returns></returns>
    protected double GenerateRandomWeight() => (Random.NextDouble() * 4 - 2).Round(RoundingPlace);

    public static TChromosome CreateRandom(Random random)
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
    public static TChromosome Mutate(Chromosome<TChromosome> chromosome, double mutationRate)
    {
        var newChromosome = chromosome.Clone();

        foreach (var kvp in chromosome.MutableStatsByName)
        {
            // TODO: Get a better mutation system going. Maybe a gaussian distribution?
            var mutationValue = chromosome.Random.NextDouble();
            if (mutationValue < mutationRate)
            {
                newChromosome.MutableStatsByName[kvp.Key] = chromosome.GenerateRandomWeight();
            }

            // double the change of normal mutations is a +- 5% mutation. 
            if (mutationValue < mutationRate * 2)
            {
                var oldValue = newChromosome.MutableStatsByName[kvp.Key];
                var change = oldValue * 0.05;
                var newValue = chromosome.Random.NextSingle() > 0.5 ? oldValue + change : oldValue - change;
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
    public static TChromosome Crossover(Chromosome<TChromosome> parent1, Chromosome<TChromosome> parent2, double crossoverRate = 0.5)
    {
        var child = parent1.Clone();
        foreach (var kvp in parent1.MutableStatsByName)
        {
            if (parent1.Random.NextDouble() < crossoverRate)
            {
                child.MutableStatsByName[kvp.Key] = parent2.MutableStatsByName[kvp.Key];
            }
        }
        return child;
    }

    public static TChromosome GetAverageChromosome(List<TChromosome> chromosomes)
    {
        if (chromosomes == null || chromosomes.Count == 0)
            throw new ArgumentException("The list of chromosomes cannot be null or empty.", nameof(chromosomes));

        var firstChromosome = chromosomes[0].Clone();
        foreach (var key in firstChromosome.MutableStatsByName.Keys.ToList())
        {
            double averageValue = chromosomes.Average(chromosome => chromosome.MutableStatsByName[key]);
            firstChromosome.MutableStatsByName[key] = averageValue;
        }

        return firstChromosome;
    }

    public static TChromosome GetStandardDeviationChromosome(List<TChromosome> chromosomes)
    {
        if (chromosomes == null || chromosomes.Count == 0)
            throw new ArgumentException("The list of chromosomes cannot be null or empty.", nameof(chromosomes));

        var firstChromosome = chromosomes[0].Clone();
        foreach (var key in firstChromosome.MutableStatsByName.Keys.ToList())
        {
            double stdValue = chromosomes.Select(chromosome => chromosome.MutableStatsByName[key])
                .StandardDeviation();
            firstChromosome.MutableStatsByName[key] = stdValue;
        }

        return firstChromosome;
    }

    /// <summary>
    /// Serializes the weights to a JSON string.
    /// </summary>
    /// <returns>A JSON string representing the weights.</returns>
    public string SerializeWeights()
    {
        return System.Text.Json.JsonSerializer.Serialize(MutableStatsByName);
    }

    /// <summary>
    /// Deserializes the weights from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string representing the weights.</param>
    public void DeserializeWeights(string json)
    {
        MutableStatsByName = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(json)
                             ?? new Dictionary<string, double>();
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            foreach (var kvp in MutableStatsByName)
            {
                hash = hash * 23 + kvp.Key.GetHashCode();
                hash = hash * 23 + kvp.Value.GetHashCode();
            }
            return hash;
        }
    }
}
