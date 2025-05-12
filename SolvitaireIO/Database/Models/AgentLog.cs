using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using SolvitaireCore;

namespace SolvitaireIO.Database.Models;

[Table("agents")]
public class AgentLog
{
    [Key]
    [JsonIgnore]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(GenerationLog))]
    public int Generation { get; set; }
    public float Count { get; set; } = 1; // TODO: implement this to avoid duplication. 
    public double Fitness { get; set; }
    public int GamesWon { get; set; }
    public int MovesMade { get; set; }
    public int GamesPlayed { get; set; }

    // Navigation property for the related GenerationLog
    [InverseProperty(nameof(GenerationLog.AgentLogs))]
    [JsonIgnore]
    public GenerationLog GenerationLog { get; set; } = null!;

    [ForeignKey(nameof(Chromosome))]
    [JsonIgnore]
    public string ChromosomeId { get; set; } = null!; 

    // Navigation property for the related ChromosomeLog
    [InverseProperty(nameof(ChromosomeLog.AgentLog))]
    public ChromosomeLog Chromosome { get; set; } = null!;
}

[Table("chromosomes")]
public class ChromosomeLog // TODO: avoid duplication
{
    [Key]
    public string StableHash { get; set; } = null!; // Use the hash as the primary key
    public double Fitness { get; set; }
    public string ChromosomeType { get; set; } = null!; // Store the type of the chromosome
    public string GeneData { get; set; } = null!; // Store the chromosome's gene data

    public string SpeciesIdentifier { get; set; } = null!;

    [InverseProperty(nameof(AgentLog.Chromosome))]
    [JsonIgnore]
    public AgentLog? AgentLog { get; set; } = null;


    private Chromosome? _chromosome = null!; // Placeholder for the actual Chromosome object
    [NotMapped]
    [JsonIgnore]
    public Chromosome Chromosome 
    {
        get =>_chromosome ??= Create(); // Lazy initialization of the Chromosome object
        set => _chromosome = value;
    }

    public static ChromosomeLog FromChromosome(Chromosome chromosome)
    {
        return new ChromosomeLog
        {
            StableHash = chromosome.GetStableHash(),
            ChromosomeType = chromosome.GetType().AssemblyQualifiedName!,
            GeneData = chromosome.ToGeneData(),
            Fitness = chromosome.Fitness,
            SpeciesIdentifier = chromosome.SpeciesIndex.ToString()
        };
    }

    public Chromosome Create()
    {
        if (_chromosome != null) 
            return _chromosome;

        var type = Type.GetType(ChromosomeType) ??
                   AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(a => a.GetTypes())
                       .FirstOrDefault(t => t.FullName == ChromosomeType);

        if (type == null)
            throw new InvalidOperationException($"Type '{ChromosomeType}' not found.");
        _chromosome = (Chromosome)Activator.CreateInstance(type)!;
        _chromosome.LoadGeneData(GeneData);
        _chromosome.Fitness = Fitness;
        _chromosome.SpeciesIndex = int.Parse(SpeciesIdentifier);
        return _chromosome;
    }
}