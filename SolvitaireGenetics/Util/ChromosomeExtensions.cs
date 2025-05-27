using SolvitaireCore;
using SolvitaireCore.ConnectFour;

namespace SolvitaireGenetics;

public static class ChromosomeExtensions
{
    public static TAgent ToGeneticAgent<TChromosome, TAgent>(this TChromosome chromosome)
        where TChromosome : Chromosome
        where TAgent : IGeneticAgent<TChromosome>
    {
        return (TAgent)Activator.CreateInstance(typeof(TAgent), chromosome)!;
    }

    public static TAgent ToGeneticAgent<TAgent>(this Chromosome chromosome, string? name = null)
        where TAgent : class
    {
        object? agent = chromosome switch
        {
            SolitaireChromosome solitaireChromosome => new SolitaireGeneticAgent(solitaireChromosome),
            ConnectFourChromosome connectFourChromosome => new ConnectFourGeneticAgent(connectFourChromosome),
            QuadraticChromosome quadraticChromosome => new QuadraticRegressionAgent(quadraticChromosome),
            _ => throw new ArgumentException($"Unsupported chromosome type: {chromosome.GetType().Name}")
        };

        if (agent is not TAgent typedAgent)
            throw new InvalidCastException($"Failed to cast the created agent to type {typeof(TAgent).Name}.");

        if (name != null)
        {
            var nameProperty = agent.GetType().GetProperty("Name", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (nameProperty != null && nameProperty.CanWrite)
            {
                nameProperty.SetValue(agent, name);
            }
            else
            {
                var backingField = agent.GetType().GetField("<Name>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                backingField?.SetValue(agent, name);
            }
        }

        return typedAgent;
    }
}

