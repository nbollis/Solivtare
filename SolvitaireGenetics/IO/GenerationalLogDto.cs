using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolvitaireGenetics;

/// <summary>
/// DTO for logging generational information.
/// </summary>
public class GenerationLogDto
{
    public int Generation { get; set; }
    public double BestFitness { get; set; }
    public double AverageFitness { get; set; }
    public double StdFitness { get; set; }
    public double AveragePairwiseDiversity { get; set; }
    public double VarianceFromAverageChromosome { get; set; }
    public int SpeciesCount { get; set; }
    public double IntraSpeciesDiversity { get; set; }
    public double InterSpeciesDiversity { get; set; }
    public Chromosome BestChromosome { get; set; } = null!;
    public Chromosome AverageChromosome { get; set; } = null!;
    public Chromosome StdChromosome { get; set; } = null!;
}