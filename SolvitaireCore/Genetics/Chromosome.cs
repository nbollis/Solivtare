namespace SolvitaireCore;

public abstract class Chromosome<TChromosome> where TChromosome : Chromosome<TChromosome>
{
    protected readonly Random Random;
    public Dictionary<string, double> MutableStatsByName { get; set; }

    protected Chromosome(Random random)
    {
        Random = random;
        MutableStatsByName = new();
    }

    public TChromosome CrossOver(TChromosome other, double crossoverRate = 0.5)
    {
        return Crossover(this, other, crossoverRate);
    }

    public TChromosome Mutate(double mutationRate)
    {
        return Mutate(this, mutationRate);
    }

    public abstract TChromosome Clone();

    /// <summary>
    /// Creates a random weight from -2 to 2. 
    /// </summary>
    /// <returns></returns>
    protected double GenerateRandomWeight()
    {
        return Random.NextDouble() * 4 - 2;
    }

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
            if (chromosome.Random.NextDouble() < mutationRate)
            {
                // TODO: Get a better mutation system going. Maybe a gaussian distribution?
                newChromosome.MutableStatsByName[kvp.Key] = chromosome.GenerateRandomWeight();
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
}
