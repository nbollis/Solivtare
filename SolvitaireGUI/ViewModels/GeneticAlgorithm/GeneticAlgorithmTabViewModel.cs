using System.Collections.Concurrent;
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
    
    public GeneticAlgorithmTabViewModel()
    {
        CurrentGeneration = 0;

        StartAlgorithmCommand = new RelayCommand(() => _ = RunAlgorithm());
        PauseCommand = new DelegateCommand(_ => PauseAlgorithm(), _ => IsAlgorithmRunning && !IsPaused);
        ResumeCommand = new DelegateCommand(_ => ResumeAlgorithm(), _ => IsAlgorithmRunning && IsPaused);

        IsAlgorithmRunning = false;
        SelectedAlgorithmType = GeneticAlgorithmType.Solitaire; // Default selection
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
            OnPropertyChanged(nameof(StartButtonText));

            // Explicitly notify the commands to re-evaluate CanExecute
            (PauseCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (ResumeCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }
    }

    public string StartButtonText => IsAlgorithmRunning ? "Restart" : "Play";

    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            _isPaused = value;
            OnPropertyChanged(nameof(IsPaused));

            // Explicitly notify the commands to re-evaluate CanExecute
            (PauseCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (ResumeCommand as DelegateCommand)?.RaiseCanExecuteChanged();
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

    public ICommand StartAlgorithmCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand ResumeCommand { get; }

    private async Task RunAlgorithm()
    {
        // Cancel any currently running algorithm
        if (IsAlgorithmRunning)
        {
            await _cancellationTokenSource!.CancelAsync();
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _pauseEvent.Set(); // Ensure the algorithm is not paused

        IsAlgorithmRunning = true;
        IsPaused = false;

        try
        {
            // Collapse the ParametersExpander
            OnPropertyChanged(nameof(IsAlgorithmRunning));

            // SetWeight up the plots
            _generationalLogs.Clear();
            SetUpPlots();

            // Create and run the genetic algorithm using the factory
            IGeneticAlgorithm? algorithm = null;
            string? filePath = null;
            if (Parameters.OutputDirectory != null)
                filePath = Path.Combine(Parameters.OutputDirectory!, "RunParameters.json");

            switch (Parameters)
            {
                case SolitaireGeneticAlgorithmParameters solitaireParams:
                    algorithm = new GeneticSolitaireAlgorithm(solitaireParams);
                    if (Parameters.OutputDirectory != null)
                        solitaireParams.SaveToFile(filePath!);
                    break;
                case QuadraticGeneticAlgorithmParameters quad:
                    algorithm = new QuadraticRegressionGeneticAlgorithm(quad);
                    if (Parameters.OutputDirectory != null)
                        quad.SaveToFile(filePath!);
                    break;
                default:
                    MessageBox.Show("Invalid parameters provided for the Genetic Algorithm.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            // Subscribe to events
            algorithm.GenerationCompleted += OnGenerationFinished;
            
            await Task.Run(() => RunEvolutionWithControl(algorithm, Parameters.Generations, _cancellationTokenSource.Token));

            MessageBox.Show("Genetic Algorithm completed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation, Usually occurs when algorithm is restarted mid run. 
            // TODO: Handle any cleanup or state reset if necessary
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

    #endregion

    #region Algorithm Type and Parameter Selection

    private bool _useChromosomeTemplate;
    private GeneticAlgorithmType _selectedAlgorithmType = GeneticAlgorithmType.Quadratic;
    private GeneticAlgorithmParameters _parameters; 
    public ChromosomeViewModel ChromosomeTemplate { get; private set; }
    public List<GeneticAlgorithmType> AlgorithmTypes { get; } = Enum.GetValues(typeof(GeneticAlgorithmType)).Cast<GeneticAlgorithmType>().ToList();
    public GeneticAlgorithmType SelectedAlgorithmType
    {
        get => _selectedAlgorithmType;
        set
        {
            if (_selectedAlgorithmType != value)
            {
                _selectedAlgorithmType = value;
                OnPropertyChanged(nameof(SelectedAlgorithmType));

                // Update the parameters based on the selected algorithm type
                Parameters = SelectedAlgorithmType switch
                {
                    GeneticAlgorithmType.Solitaire => new SolitaireGeneticAlgorithmParameters(),
                    GeneticAlgorithmType.Quadratic => new QuadraticGeneticAlgorithmParameters(),
                    _ => throw new ArgumentOutOfRangeException()
                };

                // Update the Chromosome Template to be the correct type
                ChromosomeTemplate = (SelectedAlgorithmType switch
                {
                    GeneticAlgorithmType.Solitaire => new ChromosomeViewModel(new SolitaireChromosome(Random.Shared)),
                    GeneticAlgorithmType.Quadratic => new ChromosomeViewModel(new QuadraticChromosome(Random.Shared)),
                    _ => throw new ArgumentOutOfRangeException()
                });

                OnPropertyChanged(nameof(ChromosomeTemplate));
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

    public bool UseChromosomeTemplate
    {
        get => _useChromosomeTemplate;
        set
        {
            _useChromosomeTemplate = value;
            OnPropertyChanged(nameof(UseChromosomeTemplate));

            if (value)
            {
                Parameters.TemplateChromosome = ChromosomeTemplate.BaseChromosome;
            }
            else
            {
                Parameters.TemplateChromosome = null;
            }
        }
    }

    public double TemplateChromosomeRatio
    {
        get => _parameters.TemplateInitialRatio;
        set
        {
            if (value is > 1 or < 0)
                return;

            _parameters.TemplateInitialRatio = value;
            OnPropertyChanged(nameof(TemplateChromosomeRatio));
        }
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