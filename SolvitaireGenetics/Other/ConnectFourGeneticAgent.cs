using SolvitaireCore;
using SolvitaireCore.ConnectFour;

namespace SolvitaireGenetics;

public class ConnectFourGeneticAgent : MinimaxAgent<ConnectFourGameState, ConnectFourMove>, IGeneticAgent<ConnectFourChromosome>
{
    public ConnectFourGeneticAgent(ConnectFourChromosome chromosome) : this(chromosome, null, 3) { }
    public ConnectFourGeneticAgent(ConnectFourChromosome chromosome, StateEvaluator<ConnectFourGameState, ConnectFourMove>? evaluator = null, int maxDepth = 3) : base(evaluator ?? new GeneticConnectFourEvaluator(chromosome), maxDepth)
    {
        Chromosome = chromosome;
    }

    public override string Name => "Genetic Agent";
    public ConnectFourChromosome Chromosome { get; init; }

    public IGeneticAgent<ConnectFourChromosome> CrossOver(IGeneticAgent<ConnectFourChromosome> other, double crossoverRate = 0.5)
        => new ConnectFourGeneticAgent(Chromosome.CrossOver(other.Chromosome, crossoverRate));

    public IGeneticAgent<ConnectFourChromosome> Mutate(double mutationRate)
        => new ConnectFourGeneticAgent(Chromosome.Mutate<ConnectFourChromosome>(mutationRate));

    public IGeneticAgent<ConnectFourChromosome> Clone()
        => new ConnectFourGeneticAgent(Chromosome.Clone<ConnectFourChromosome>());
}