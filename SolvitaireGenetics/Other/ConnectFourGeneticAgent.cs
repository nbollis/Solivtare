using SolvitaireCore;
using SolvitaireCore.ConnectFour;

namespace SolvitaireGenetics;

public class ConnectFourGeneticAgent(ConnectFourChromosome chromosome, StateEvaluator<ConnectFourGameState, ConnectFourMove>? evaluator = null, int maxDepth = 3)
    : MinimaxAgent<ConnectFourGameState, ConnectFourMove>(evaluator ?? new GeneticConnectFourEvaluator(chromosome), maxDepth), 
        IGeneticAgent<ConnectFourChromosome>
{
    public ConnectFourGeneticAgent(ConnectFourChromosome chromosome) : this(chromosome, null, 3) { }

    public override string Name { get; } = "Genetic Agent";
    public ConnectFourChromosome Chromosome { get; init; } = chromosome;

    public double Fitness 
    {
        get => Chromosome.Fitness;
        set => Chromosome.Fitness = value;
    }

    public IGeneticAgent<ConnectFourChromosome> CrossOver(IGeneticAgent<ConnectFourChromosome> other, double crossoverRate = 0.5)
        => new ConnectFourGeneticAgent(Chromosome.CrossOver(other.Chromosome, crossoverRate));

    public IGeneticAgent<ConnectFourChromosome> Mutate(double mutationRate)
        => new ConnectFourGeneticAgent(Chromosome.Mutate<ConnectFourChromosome>(mutationRate));

    public IGeneticAgent<ConnectFourChromosome> Clone()
        => new ConnectFourGeneticAgent(Chromosome.Clone<ConnectFourChromosome>());
}