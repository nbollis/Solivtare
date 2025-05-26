using SolvitaireCore;

namespace SolvitaireGenetics;

public class SolitaireGeneticAgent : MaximizingAgent<SolitaireGameState, SolitaireMove>, IGeneticAgent<SolitaireChromosome>
{
    public override string Name => "Solitaire Genetic Agent";
    public SolitaireChromosome Chromosome { get; init; }
    public SolitaireGeneticAgent(SolitaireChromosome chromosome)
        : this(chromosome, null, 3)
    {
    }
    public SolitaireGeneticAgent(SolitaireChromosome chromosome, StateEvaluator<SolitaireGameState, SolitaireMove>? evaluator = null, int maxDepth = 3) 
        : base(evaluator ?? new GeneticSolitaireEvaluator(chromosome), maxDepth)
    {
        Chromosome = chromosome;
    }

    public IGeneticAgent<SolitaireChromosome> CrossOver(IGeneticAgent<SolitaireChromosome> other, double crossoverRate = 0.5)
        => new SolitaireGeneticAgent(Chromosome.CrossOver(other.Chromosome, crossoverRate));

    public IGeneticAgent<SolitaireChromosome> Mutate(double mutationRate)
        => new SolitaireGeneticAgent(Chromosome.Mutate<SolitaireChromosome>(mutationRate));

    public IGeneticAgent<SolitaireChromosome> Clone()
        => new SolitaireGeneticAgent(Chromosome.Clone<SolitaireChromosome>());
}