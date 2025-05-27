using SolvitaireCore;

namespace SolvitaireGenetics;

public static class ChromosomeExtensions
{
    public static TAgent ToGeneticAgent<TChromosome, TAgent>(this TChromosome chromosome)
        where TChromosome : Chromosome
        where TAgent : IGeneticAgent<TChromosome>
    {
        return (TAgent)Activator.CreateInstance(typeof(TAgent), chromosome)!;
    }

    public static IGeneticAgent<Chromosome> ToGeneticAgent(this Chromosome chromosome, string? name = null)
    {
        var agentType = typeof(IGeneticAgent<>).MakeGenericType(chromosome.GetType());
        var agent = (IGeneticAgent<Chromosome>)Activator.CreateInstance(agentType, chromosome)!;

        if (name != null)
        {
            var nameProperty = agentType.GetProperty("Name", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (nameProperty != null && nameProperty.CanWrite == false)
            {
                var backingField = agentType.GetField("<Name>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                backingField?.SetValue(agent, name);
            }
        }

        return agent;
    }
}

