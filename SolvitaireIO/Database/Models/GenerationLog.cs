using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolvitaireIO.Database.Models;

[Table("GenerationLogs")]
public class GenerationLog
{
    [Key]
    public int Generation { get; set; }
    public double BestFitness { get; set; }
    public double AverageFitness { get; set; }
    public double StdFitness { get; set; }
    public double AveragePairwiseDiversity { get; set; }
    public double VarianceFromAverageChromosome { get; set; }
    public int SpeciesCount { get; set; }
    public double IntraSpeciesDiversity { get; set; }
    public double InterSpeciesDiversity { get; set; }
    public string BestChromosomeJson { get; set; } // Store Chromosome as JSON for now
    public string AverageChromosmeJson { get; set; }
    public string StdChromosmeJson { get; set; }
}