using System.Collections.Concurrent;
using System.IO;
using SolvitaireGenetics;
using System.Windows;
using System.Windows.Input;
using ScottPlot.WPF;
using SolvitairePlotting;
using ScottPlot;
using System.Threading;
using ScottPlot.Finance;

namespace SolvitaireGUI;

public class GeneticAlgorithmTabViewModel : BaseViewModel
{
    
    public GeneticAlgorithmTabViewModel()
    {
        _writeOutput = true;
        CurrentGeneration = 0;
        SelectedAlgorithmType = GeneticAlgorithmType.Solitaire; // Default selection
        UpdateParameters();

        RunAlgorithmCommand = new RelayCommand(RunAlgorithm);
    }

    public GeneticAlgorithmTabViewModel(GeneticAlgorithmParameters parameters) : this()
    {
        Parameters = parameters;
        SetUpPlots();
    }

    #region Algorithm Running

    private int _currentGeneration;
    private bool _isAlgorithmRunning;
    private bool _isPaused;
    private bool _writeOutput;
    private CancellationTokenSource _cancellationTokenSource;
    private ManualResetEventSlim _pauseEvent = new(true); // Initially not paused
    private readonly ConcurrentQueue<GenerationLogDto> _generationalLogs = new();

    public bool IsAlgorithmRunning
    {
        get => _isAlgorithmRunning;
        set
        {
            _isAlgorithmRunning = value;
            OnPropertyChanged(nameof(IsAlgorithmRunning));
        }
    }
    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            _isPaused = value;
            OnPropertyChanged(nameof(IsPaused));
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

    public bool WriteOutput
    {
        get => _writeOutput;
        set
        {
            _writeOutput = value;
            OnPropertyChanged(nameof(WriteOutput));
        }
    }

    public ICommand RunAlgorithmCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand ResumeCommand { get; }
    public ICommand StopCommand { get; }

    private async void RunAlgorithm()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _pauseEvent.Set(); // Ensure the algorithm is not paused
        IsAlgorithmRunning = true;
        IsPaused = false;

        try
        {
            // Collapse the ParametersExpander
            OnPropertyChanged(nameof(IsAlgorithmRunning));

            // Set up the plots
            _generationalLogs.Clear();
            SetUpPlots();

            // Create and run the genetic algorithm using the factory
            IGeneticAlgorithm? algorithm = null;
            string? filePath = null;
            if (WriteOutput)
                filePath = Path.Combine(Parameters.OutputDirectory!, "RunParameters.json");
            switch (Parameters)
            {
                case SolitaireGeneticAlgorithmParameters solitaireParams:
                    algorithm = new GeneticSolitaireAlgorithm(solitaireParams);
                    if (WriteOutput)
                        solitaireParams.SaveToFile(filePath!);
                    break;
                case QuadraticGeneticAlgorithmParameters quad:
                    algorithm = new QuadraticRegressionGeneticAlgorithm(quad);
                    if (WriteOutput)
                        quad.SaveToFile(filePath!);
                    break;
                default:
                    MessageBox.Show("Invalid parameters provided for the Genetic Algorithm.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            // Subscribe to events
            algorithm.GenerationCompleted += OnGenerationFinished;

            if (WriteOutput)
            {
                
                Parameters.SaveToFile(filePath);
            }
            
            await Task.Run(() => RunEvolutionWithControl(algorithm, Parameters.Generations, _cancellationTokenSource.Token));

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

    private void RunEvolutionWithControl(IGeneticAlgorithm algorithm, int generations, CancellationToken cancellationToken)
    {
        for (int generation = 0; generation < generations; generation++)
        {
            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            // Wait if paused
            _pauseEvent.Wait(cancellationToken);

            // Run one generation
            algorithm.RunEvolution(1);
        }
    }

    private void PauseAlgorithm()
    {
        _pauseEvent.Reset(); // Pause the algorithm
        IsPaused = true;
    }

    private void ResumeAlgorithm()
    {
        _pauseEvent.Set(); // Resume the algorithm
        IsPaused = false;
    }

    private void StopAlgorithm()
    {
        _cancellationTokenSource?.Cancel(); // Stop the algorithm
        IsAlgorithmRunning = false;
        IsPaused = false;
    }

    #endregion

    #region Plotting

    public WpfPlot AverageStatByGeneration { get; set; } = new WpfPlot();
    public WpfPlot FitnessByGeneration { get; set; } = new WpfPlot();

    private void SetUpPlots()
    {
        FitnessByGeneration.Plot.Clear();
        FitnessByGeneration.Plot.Axes.SetLimits(0, Parameters.Generations - 1, -1, 1.5);
        FitnessByGeneration.Plot.XLabel("Generation");
        FitnessByGeneration.Plot.YLabel("Fitness");
        FitnessByGeneration.Refresh();

        AverageStatByGeneration.Plot.Clear();
        AverageStatByGeneration.Plot.Axes.SetLimits(0, Parameters.Generations - 1, -3, 3);
        AverageStatByGeneration.Plot.XLabel("Generation");
        AverageStatByGeneration.Plot.YLabel("Weight");
        AverageStatByGeneration.Refresh();
    }

    private void OnGenerationFinished(int generation, GenerationLogDto generationLog)
    {
        int movingAverageAmount = 1;
        CurrentGeneration = generation;
        _generationalLogs.Enqueue(generationLog);

        var sortedLogs = _generationalLogs.ToList();

        // Extract fitness data in a single pass
        var bestFitness = new double[sortedLogs.Count];
        var averageFitness = new double[sortedLogs.Count];
        var stdFitness = new double[sortedLogs.Count];
        var statNames = generationLog.AverageChromosome.MutableStatsByName.Keys.ToArray();
        var averageStatValues = new Dictionary<string, double[]>();
        var bestStatValues = new Dictionary<string, double[]>();

        for (int i = 0; i < sortedLogs.Count; i++)
        {
            bestFitness[i] = sortedLogs[i].BestFitness;
            averageFitness[i] = sortedLogs[i].AverageFitness;
            stdFitness[i] = sortedLogs[i].StdFitness;
        }

        // Extract values for each stat  
        foreach (var statName in statNames)
        {
            var averageValues = sortedLogs.Select(log => log.AverageChromosome.MutableStatsByName[statName]).ToArray();
            var bestValues = sortedLogs.Select(log => log.BestChromosome.MutableStatsByName[statName]).ToArray();
            averageStatValues[statName] = averageValues;
            bestStatValues[statName] = bestValues;
        }

        lock (AverageStatByGeneration.Plot.Sync)
        {
            AverageStatByGeneration.Plot.Clear();
            
            // Add extracted values to the plots  
            for (int i = 0; i < statNames.Length; i++)
            {
                var statName = statNames[i];

                // Generate a consistent color for both average and best plots  
                var color = PlottingConstants.AllColors[i];

                // Add Average scatter plot for each stat  
                var scatter = AverageStatByGeneration.Plot.Add.Signal(averageStatValues[statName]);
                scatter.LegendText = statName;
                scatter.LinePattern = LinePattern.Solid;
                scatter.Color = color;
                scatter.LineWidth = 2;

                // Add Best scatter plot for each stat  
                var bestScatter = AverageStatByGeneration.Plot.Add.Signal(bestStatValues[statName]);
                bestScatter.LinePattern = LinePattern.DenselyDashed;
                bestScatter.Color = color;
            }

            // Add Target lines to quadratic. 
            if (Parameters is QuadraticGeneticAlgorithmParameters quad)
            {
                int colorIndex = -1;
                var aLine = new ScottPlot.Plottables.LinePlot()
                {
                    Start = new Coordinates(0, quad.CorrectA),
                    End = new Coordinates(quad.Generations, quad.CorrectA),
                    LineColor = PlottingConstants.AllColors[++colorIndex],
                    LinePattern = LinePattern.Dotted,
                    LineWidth = 1.5f,
                };

                var bLine = new ScottPlot.Plottables.LinePlot()
                {
                    Start = new Coordinates(0, quad.CorrectB),
                    End = new Coordinates(quad.Generations, quad.CorrectB),
                    LineColor = PlottingConstants.AllColors[++colorIndex],
                    LinePattern = LinePattern.Dotted,
                    LineWidth = 1.5f,
                };

                var cLine = new ScottPlot.Plottables.LinePlot()
                {
                    Start = new Coordinates(0, quad.CorrectC),
                    End = new Coordinates(quad.Generations, quad.CorrectC),
                    LineColor = PlottingConstants.AllColors[++colorIndex],
                    LinePattern = LinePattern.Dotted,
                    LineWidth = 1.5f,
                };

                var yIntLine = new ScottPlot.Plottables.LinePlot()
                {
                    Start = new Coordinates(0, quad.CorrectIntercept),
                    End = new Coordinates(quad.Generations, quad.CorrectIntercept),
                    LineColor = PlottingConstants.AllColors[++colorIndex],
                    LinePattern = LinePattern.Dotted,
                    LineWidth = 1.5f,
                };

                AverageStatByGeneration.Plot.Add.Plottable(aLine);
                AverageStatByGeneration.Plot.Add.Plottable(bLine);
                AverageStatByGeneration.Plot.Add.Plottable(cLine);
                AverageStatByGeneration.Plot.Add.Plottable(yIntLine);
            }

            AverageStatByGeneration.Plot.ShowLegend(Alignment.UpperLeft, Orientation.Horizontal);
        }
        
        lock (FitnessByGeneration.Plot.Sync)
        {
            FitnessByGeneration.Plot.Clear();

            // Fitness Plot
            var bestSig = FitnessByGeneration.Plot.Add.Signal(bestFitness);
            bestSig.LegendText = "Best Fitness";
            bestSig.LineWidth = 2;

            var avgSig = FitnessByGeneration.Plot.Add.Signal(averageFitness);
            avgSig.LegendText = "Average Fitness";
            avgSig.LineWidth = 2;

            var stdSig = FitnessByGeneration.Plot.Add.Signal(stdFitness);
            stdSig.LegendText = "Std Fitness";
            stdSig.LineWidth = 2;

            FitnessByGeneration.Plot.ShowLegend(Alignment.UpperLeft, Orientation.Vertical);
        }

        // Refresh the plots
        FitnessByGeneration.Refresh();
        AverageStatByGeneration.Refresh();

        OnPropertyChanged(nameof(FitnessByGeneration));
    }

    #endregion

    #region Algorithm Type and Parameter Selection

    private GeneticAlgorithmType _selectedAlgorithmType;
    private GeneticAlgorithmParameters _parameters;

    public GeneticAlgorithmType SelectedAlgorithmType
    {
        get => _selectedAlgorithmType;
        set
        {
            if (_selectedAlgorithmType != value)
            {
                _selectedAlgorithmType = value;
                OnPropertyChanged(nameof(SelectedAlgorithmType));
                UpdateParameters();
            }
        }
    }

    public GeneticAlgorithmParameters Parameters
    {
        get => _parameters;
        set
        {
            _parameters = value;
            OnPropertyChanged(nameof(Parameters));
        }
    }

    public List<GeneticAlgorithmType> AlgorithmTypes { get; } = Enum.GetValues(typeof(GeneticAlgorithmType)).Cast<GeneticAlgorithmType>().ToList();

    private void UpdateParameters()
    {
        Parameters = SelectedAlgorithmType switch
        {
            GeneticAlgorithmType.Solitaire => new SolitaireGeneticAlgorithmParameters(),
            GeneticAlgorithmType.Quadratic => new QuadraticGeneticAlgorithmParameters(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    #endregion

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

    private double[] CalculateMovingAverage(double[] values, int windowSize)
    {
        if (values.Length < windowSize)
            return values;

        var movingAverage = new double[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            int start = Math.Max(0, i - windowSize + 1);
            int count = i - start + 1;
            movingAverage[i] = values.Skip(start).Take(count).Average();
        }
        return movingAverage;
    }
}

public class GeneticAlgorithmTabModel : GeneticAlgorithmTabViewModel
{
    public static GeneticAlgorithmTabModel Instance => new();

    public GeneticAlgorithmTabModel() : base(new SolitaireGeneticAlgorithmParameters())
    {

    }
}