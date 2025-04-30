using System.IO;
using SolvitaireGenetics;
using System.Windows;
using System.Windows.Input;
using ScottPlot.WPF;
using SolvitairePlotting;
using ScottPlot;

namespace SolvitaireGUI;

public class GeneticAlgorithmTabViewModel : BaseViewModel
{
    private int _currentGeneration;
    private bool _isAlgorithmRunning;
    private List<GenerationLogDto> _generationalLogs = new();


    public GeneticAlgorithmParameters Parameters { get; set; }
    public bool IsAlgorithmRunning
    {
        get => _isAlgorithmRunning;
        set
        {
            _isAlgorithmRunning = value;
            OnPropertyChanged(nameof(IsAlgorithmRunning));
        }
    }

    public int CurrentGeneration
    {
        get => _currentGeneration;
        set
        {
            _currentGeneration = value;
            OnPropertyChanged(nameof(CurrentGeneration));
        }

    }

    
    public WpfPlot AverageStatByGeneration { get; set; } = new WpfPlot();
    public WpfPlot FitnessByGeneration { get; set; } = new WpfPlot();

    public ICommand RunAlgorithmCommand { get; }

    public GeneticAlgorithmTabViewModel(GeneticAlgorithmParameters parameters)
    {
        CurrentGeneration = 0;
        Parameters = parameters;
        RunAlgorithmCommand = new RelayCommand(RunAlgorithm); 
        SetUpPlots();
    }

    private async void RunAlgorithm()
    {
        IsAlgorithmRunning = true;

        try
        {
            // Collapse the ParametersExpander
            OnPropertyChanged(nameof(IsAlgorithmRunning));

            // Set up the plots
            SetUpPlots();

            // Create and run the genetic algorithm using the factory
            IGeneticAlgorithm? algorithm = null;

            if (Parameters is SolitaireGeneticAlgorithmParameters solitaireParams)
            {
                algorithm = new GeneticSolitaireAlgorithm(solitaireParams);
            }
            else
            {
                MessageBox.Show("Invalid parameters provided for the Genetic Algorithm.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Subscribe to events
            algorithm.GenerationCompleted += OnGenerationCompleted;

            algorithm.WriteParameters();
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

    private void SetUpPlots()
    {
        FitnessByGeneration.Plot.Clear();
        FitnessByGeneration.Plot.Axes.SetLimits(0, Parameters.Generations-1, 0, 1);
        FitnessByGeneration.Plot.XLabel("Generation");
        FitnessByGeneration.Plot.YLabel("Fitness");
        FitnessByGeneration.Refresh();

        AverageStatByGeneration.Plot.Clear();
        AverageStatByGeneration.Plot.Axes.SetLimits(0, Parameters.Generations-1, 0, 1);
        AverageStatByGeneration.Plot.XLabel("Generation");
        AverageStatByGeneration.Plot.YLabel("Weight");
        AverageStatByGeneration.Refresh();
    }

    private void OnGenerationCompleted(int generation, GenerationLogDto generationLog)
    {
        CurrentGeneration = generation;
        _generationalLogs.Add(generationLog);

        FitnessByGeneration.Plot.Clear(); 
        AverageStatByGeneration.Plot.Clear();

        // Sort generationalLogs once to avoid repeated sorting
        var sortedLogs = _generationalLogs.OrderBy(p => p.Generation).ToList();
        int[] generations = sortedLogs.Select(p => p.Generation).ToArray();

        // Extract fitness data in a single pass
        var bestFitness = new double[sortedLogs.Count];
        var averageFitness = new double[sortedLogs.Count];
        var stdFitness = new double[sortedLogs.Count];
        var avgChromosomes = new ChromosomeDto[sortedLogs.Count];
        var bestChromosomes = new ChromosomeDto[sortedLogs.Count];

        for (int i = 0; i < sortedLogs.Count; i++)
        {
            bestFitness[i] = sortedLogs[i].BestFitness;
            averageFitness[i] = sortedLogs[i].AverageFitness;
            stdFitness[i] = sortedLogs[i].StdFitness;
            avgChromosomes[i] = sortedLogs[i].AverageChromosome;
            bestChromosomes[i] = sortedLogs[i].BestChromosome;
        }

        // Average Stat Plot  
        var statNames = generationLog.AverageChromosome.Weights.Keys.ToArray();
        for (int i = 0; i < statNames.Length; i++)
        {
            var statName = statNames[i];

            // Generate a consistent color for both average and best plots  
            var color = PlottingConstants.AllColors[i];

            // Add Average scatter plot for each stat  
            var scatter = AverageStatByGeneration.Plot.Add.Scatter(
                generations.Select(g => (double)g).ToArray(),
                sortedLogs.Select(log => log.AverageChromosome.Weights[statName]).ToArray());
            scatter.LegendText = statName;
            scatter.LinePattern = LinePattern.Solid;
            scatter.Color = color;

            // Add Best scatter plot for each stat  
            var bestScatter = AverageStatByGeneration.Plot.Add.Scatter(
                generations.Select(g => (double)g).ToArray(),
                sortedLogs.Select(log => log.BestChromosome.Weights[statName]).ToArray());
            bestScatter.LinePattern = LinePattern.Dashed;
            bestScatter.Color = color;
        }
        AverageStatByGeneration.Plot.ShowLegend(Alignment.UpperLeft, Orientation.Horizontal);


        // Fitness Plot
        var bestSig = FitnessByGeneration.Plot.Add.Scatter(generations, bestFitness);
        bestSig.LegendText = "Best Fitness";

        var avgSig = FitnessByGeneration.Plot.Add.Scatter(generations, averageFitness);
        avgSig.LegendText = "Average Fitness";

        var stdSig = FitnessByGeneration.Plot.Add.Scatter(generations, stdFitness);
        stdSig.LegendText = "Std Fitness";

        FitnessByGeneration.Plot.ShowLegend(Alignment.UpperLeft, Orientation.Vertical);

        // Refresh the plots
        FitnessByGeneration.Refresh();
        AverageStatByGeneration.Refresh();

        OnPropertyChanged(nameof(FitnessByGeneration));
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
                    SetUpPlots();
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

public class GeneticAlgorithmTabModel : GeneticAlgorithmTabViewModel
{
    public static GeneticAlgorithmTabModel Instance => new();

    public GeneticAlgorithmTabModel() : base(new SolitaireGeneticAlgorithmParameters())
    {

    }
}