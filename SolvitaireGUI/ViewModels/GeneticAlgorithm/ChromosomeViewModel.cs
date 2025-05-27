using System.Collections.ObjectModel;
using SolvitaireCore;

namespace SolvitaireGUI;

/// <summary>
/// ViewModel for the SolitaireChromosome class, allowing dynamic interaction with its weights.
/// </summary>
public class ChromosomeViewModel : BaseViewModel
{
    public Chromosome BaseChromosome { get; }

    /// <summary>
    /// Observable collection of weight names and their values.
    /// </summary>
    public ObservableCollection<ChromosomeWeight> Weights { get; } = new();

    /// <summary>
    /// Initializes a new instance of the ChromosomeViewModel.
    /// </summary>
    /// <param name="chromosome">The Chromosome instance to wrap.</param>
    public ChromosomeViewModel(Chromosome chromosome)
    {
        BaseChromosome = chromosome;
        foreach (var kvp in BaseChromosome.MutableStatsByName)
        {
            Weights.Add(new ChromosomeWeight(kvp.Key, kvp.Value, BaseChromosome.MutableStatsByName));
        }
    }
}

/// <summary>
/// Represents a single weight in the Chromosome.
/// </summary>
public class ChromosomeWeight : BaseViewModel
{
    private string _name;
    private double _value;
    private readonly Dictionary<string, double> _baseWeights;

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public double Value
    {
        get => _value;
        set
        {
            _value = value;
            _baseWeights[_name] = value;
            OnPropertyChanged(nameof(Value));
        }
    }

    public ChromosomeWeight(string name, double value, Dictionary<string, double> baseWeights)
    {
        _name = name;
        _value = value;
        _baseWeights = baseWeights;
    }
}

