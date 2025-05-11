using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolvitaireIO.Database.Models;

[Table("generations")]
public class GenerationLog
{
    [Key]
    public int Generation { get; set; }
    public DateTime GenerationFinishedTime { get; set; }
    public double BestFitness { get; set; }
    public double AverageFitness { get; set; }
    public double StdFitness { get; set; }
    public double AveragePairwiseDiversity { get; set; }
    public double VarianceFromAverageChromosome { get; set; }
    public int SpeciesCount { get; set; }
    public double IntraSpeciesDiversity { get; set; }
    public double InterSpeciesDiversity { get; set; }

    // Navigation properties for best, average, and std chromosomes
    [ForeignKey(nameof(BestChromosome))]
    public string BestChromosomeId { get; set; }
    public ChromosomeLog? BestChromosome { get; set; }

    [ForeignKey(nameof(AverageChromosome))]
    public string AverageChromosomeId { get; set; }
    public ChromosomeLog? AverageChromosome { get; set; }

    [ForeignKey(nameof(StdChromosome))]
    public string StdChromosomeId { get; set; }
    public ChromosomeLog? StdChromosome { get; set; }


    // Navigation property for related AgentRepository
    [InverseProperty(nameof(AgentLog.GenerationLog))]
    public List<AgentLog> AgentLogs { get; set; } = new();
}