using System.IO;
using SolvitaireGenetics;
using System.Windows;
using System.Windows.Input;

namespace SolvitaireGUI;

public class GeneticAlgorithmViewModel : BaseViewModel
{
    public GeneticAlgorithmParameters Parameters { get; set; }
    private bool _isAlgorithmRunning;
    public bool IsAlgorithmRunning
    {
        get => _isAlgorithmRunning;
        set
        {
            _isAlgorithmRunning = value;
            OnPropertyChanged(nameof(IsAlgorithmRunning));
        }
    }

    public ICommand RunAlgorithmCommand { get; }

    public GeneticAlgorithmViewModel(GeneticAlgorithmParameters parameters)
    {
        Parameters = parameters;
        RunAlgorithmCommand = new RelayCommand(RunAlgorithm);
    }

    private async void RunAlgorithm()
    {
        IsAlgorithmRunning = true;

        try
        {
            // Collapse the ParametersExpander
            OnPropertyChanged(nameof(IsAlgorithmRunning));

            // Create and run the genetic algorithm using the factory
            GeneticSolitaireAlgorithm? algorithm = null;

            if (Parameters is SolitaireGeneticAlgorithmParameters solitaireParams)
            {
                algorithm = new GeneticSolitaireAlgorithm(solitaireParams);
            }
            else
            {
                MessageBox.Show("Invalid parameters provided for the Genetic Algorithm.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await Task.Run(() => algorithm.RunEvolution(Parameters.Generations));

            MessageBox.Show("Genetic Algorithm completed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while running the algorithm: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsAlgorithmRunning = false;
        }
    }

    public void HandleFileDrop(object paths)
    {
        if (paths is string[] { Length: > 0 } files)
        {
            var filePath = files[0];
            if (Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // Dynamically load the correct parameter type, Update the Parameters property
                    Parameters = GeneticAlgorithmParameters.LoadFromFile(filePath);
                    OnPropertyChanged(nameof(Parameters));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load parameters from the dropped file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please drop a valid JSON file.", "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}

public class GeneticAlgorithmModel : GeneticAlgorithmViewModel
{
    public static GeneticAlgorithmModel Instance => new();

    public GeneticAlgorithmModel() : base(new SolitaireGeneticAlgorithmParameters())
    {

    }
}