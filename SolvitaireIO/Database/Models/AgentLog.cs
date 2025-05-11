using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SolvitaireCore;

namespace SolvitaireIO.Database.Models;

[Table("agents")]
public class AgentLog
{
    [Key]
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
    public GenerationLog GenerationLog { get; set; } = null!;

    [ForeignKey(nameof(Chromosome))]
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

    [InverseProperty(nameof(AgentLog.Chromosome))]
    public AgentLog? AgentLog { get; set; } = null;


    private Chromosome? _chromosome = null!; // Placeholder for the actual Chromosome object
    [NotMapped] public Chromosome Chromosome => _chromosome ??= Create(); // Lazy initialization of the Chromosome object

    public static ChromosomeLog FromChromosome(Chromosome chromosome, double fitness = -1)
    {
        return new ChromosomeLog
        {
            StableHash = chromosome.GetStableHash(),
            ChromosomeType = chromosome.GetType().FullName!,
            GeneData = chromosome.ToGeneData(),
            Fitness = fitness
        };
    }

    public Chromosome Create()
    {
        if (_chromosome != null) 
            return _chromosome;

        var type = Type.GetType(ChromosomeType);
        if (type == null)
            throw new InvalidOperationException($"Type '{ChromosomeType}' not found.");
        _chromosome = (Chromosome)Activator.CreateInstance(type)!;
        _chromosome.LoadGeneData(GeneData);
        _chromosome.Fitness = Fitness;
        return _chromosome;
    }
}