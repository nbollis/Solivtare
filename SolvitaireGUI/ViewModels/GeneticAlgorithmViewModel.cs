using System.IO;
using SolvitaireGenetics;
using System.Windows;
using System.Windows.Input;

namespace SolvitaireGUI;

public class GeneticAlgorithmViewModel : BaseViewModel
{
    public SolitaireGeneticAlgorithmParametersViewModel Parameters { get; set; }
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

    public GeneticAlgorithmViewModel()
    {
        Parameters = new SolitaireGeneticAlgorithmParametersViewModel();
        RunAlgorithmCommand = new RelayCommand(RunAlgorithm); 

    }

    private async void RunAlgorithm()
    {
        IsAlgorithmRunning = true;

        try
        {
            // Collapse the ParametersExpander
            OnPropertyChanged(nameof(IsAlgorithmRunning));

            // Create and run the genetic algorithm
            var parameters = Parameters; // Access the parameters from the view model
            var algorithm = new GeneticSolitaireAlgorithm(Parameters.GetParameters());

            await Task.Run(() => algorithm.RunEvolution(parameters.Generations));

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
                    Parameters =
                        new SolitaireGeneticAlgorithmParametersViewModel(
                            SolitaireGeneticAlgorithmParameters.LoadFromFile(filePath));
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

    public GeneticAlgorithmModel() : base()
    {

    }
}