using System.Collections.ObjectModel;
using SolvitaireGenetics;

namespace SolvitaireGUI;

/// <summary>
/// ViewModel for the SolitaireChromosome class, allowing dynamic interaction with its weights.
/// </summary>
public class SolitaireChromosomeViewModel : BaseViewModel
{
    public SolitaireChromosome BaseChromosome { get; }

    /// <summary>
    /// Observable collection of weight names and their values.
    /// </summary>
    public ObservableCollection<ChromosomeWeight> Weights { get; } = new();

    /// <summary>
    /// Initializes a new instance of the SolitaireChromosomeViewModel.
    /// </summary>
    /// <param name="chromosome">The SolitaireChromosome instance to wrap.</param>
    public SolitaireChromosomeViewModel(SolitaireChromosome chromosome)
    {
        BaseChromosome = chromosome;
        Sync();
    }

    /// <summary>
    /// Synchronizes the ViewModel with the underlying SolitaireChromosome.
    /// </summary>
    public void Sync()
    {
        Weights.Clear();
        foreach (var kvp in BaseChromosome.MutableStatsByName)
        {
            Weights.Add(new ChromosomeWeight(kvp.Key, kvp.Value));
        }

        OnPropertyChanged(nameof(Weights));
    }

    /// <summary>
    /// Updates the weight in the underlying SolitaireChromosome and synchronizes the ViewModel.
    /// </summary>
    /// <param name="weightName">The name of the weight to update.</param>
    /// <param name="value">The new value for the weight.</param>
    public void UpdateWeight(string weightName, double value)
    {
        BaseChromosome.SetWeight(weightName, value);
        Sync();
    }

    /// <summary>
    /// Updates multiple weights in the underlying SolitaireChromosome and synchronizes the ViewModel.
    /// </summary>
    /// <param name="weights">A dictionary of weight names and their values.</param>
    public void UpdateWeights(Dictionary<string, double> weights)
    {
        foreach (var kvp in weights)
        {
            BaseChromosome.SetWeight(kvp.Key, kvp.Value);
        }

        Sync();
    }
}

/// <summary>
/// Represents a single weight in the SolitaireChromosome.
/// </summary>
public class ChromosomeWeight : BaseViewModel
{
    private string _name;
    private double _value;

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
            OnPropertyChanged(nameof(Value));
        }
    }

    public ChromosomeWeight(string name, double value)
    {
        _name = name;
        _value = value;
    }
}

