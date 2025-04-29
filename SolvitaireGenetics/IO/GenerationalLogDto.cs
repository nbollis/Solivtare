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
    public ChromosomeDto BestChromosome { get; set; } = null!;
    public ChromosomeDto AverageChromosome { get; set; } = null!;
    public ChromosomeDto StdChromosome { get; set; } = null!;
}