using System.Collections.Concurrent;
using System.IO;
using SolvitaireGenetics;
using System.Windows;
using System.Windows.Input;
using ScottPlot.WPF;
using SolvitairePlotting;
using ScottPlot;
using System.Security.Cryptography;
using SolvitaireCore;
using SolvitaireGenetics.IO;
using SolvitaireIO.Database.Models;

namespace SolvitaireGUI;

public class GeneticAlgorithmTabViewModel : BaseViewModel
{
    
    public GeneticAlgorithmTabViewModel()
    {
        CurrentGeneration = 0;

        StartAlgorithmCommand = new RelayCommand(() => _ = RunAlgorithm());
        PauseCommand = new DelegateCommand(_ => PauseAlgorithm(), _ => IsAlgorithmRunning && !IsPaused);
        ResumeCommand = new DelegateCommand(_ => ResumeAlgorithm(), _ => IsAlgorithmRunning && IsPaused);
        StopCommand = new DelegateCommand(_ => StopAlgorithm(), _ => IsAlgorithmRunning);
        ThanosSnapCommand = new DelegateCommand(_ => ExecuteThanosSnap(), _ => IsAlgorithmRunning && _algorithm is not null && !_algorithm.ThanosSnapTriggered);


        IsAlgorithmRunning = false;
        SelectedAlgorithmType = GeneticAlgorithmType.Solitaire; // Default selection
    }

    public GeneticAlgorithmTabViewModel(GeneticAlgorithmParameters parameters) : this()
    {
        Parameters = parameters.ToViewModel();
        SetUpPlots();
    }

    #region Algorithm Running

    private int _currentGeneration;
    private bool _isAlgorithmRunning;
    private bool _isPaused; 
    private Task? _runningTask;
    private CancellationTokenSource _cancellationTokenSource;
    private ManualResetEventSlim _pauseEvent = new(true); // Initially not paused
    private readonly ConcurrentQueue<GenerationLog> _generationalLogs = new();

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
            (StopCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (ThanosSnapCommand as DelegateCommand)?.RaiseCanExecuteChanged();
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
            (StopCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (ThanosSnapCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }
    }

    public bool ThanosSnapTriggered
    {
        get => _algorithm?.ThanosSnapTriggered ?? false;
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
    public ICommand StopCommand { get; }
    public ICommand ThanosSnapCommand { get; }

    private IGeneticAlgorithm? _algorithm;
    private async Task RunAlgorithm()
    {
        // Cancel any currently running algorithm
        if (IsAlgorithmRunning)
        {
            await StopRunningTaskAsync();
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _pauseEvent.Set(); // Ensure the algorithm is not paused

        IsAlgorithmRunning = true;
        IsPaused = false;

        // IF previous state was loaded on file drop, plots will already be set up
        if (_generationalLogs.IsEmpty)
            SetUpPlots();

        // Create algorithm and subscribe to events. 
        _algorithm = GetAlgorithm();
        _algorithm.GenerationCompleted += OnGenerationFinished;

        // Restore cached generation data if this is a continuation. 
        var previousGenerationLogs = GetAlgorithm().Logger.ReadGenerationLogs();
        if (previousGenerationLogs.Count != _generationalLogs.Count)
        {
            _generationalLogs.Clear(); // Clear the queue if the number of logs is different
            for (int i = 0; i < previousGenerationLogs.Count - 1; i++)
            {
                _generationalLogs.Enqueue(previousGenerationLogs[i]);
            }

            OnGenerationFinished(previousGenerationLogs[^1].Generation, previousGenerationLogs[^1]);
        }
        try
        {
            var token = _cancellationTokenSource.Token;
            _runningTask = Task.Run(() => RunEvolutionWithControl(_algorithm, Parameters.Generations, token), token);
            await _runningTask;

            if (!token.IsCancellationRequested)
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
            _generationalLogs.Clear();
            IsAlgorithmRunning = false;

            if (_algorithm is not null)
            {
                _algorithm.Logger.CreateTsvSummaries(Parameters.OutputDirectory!);
                _algorithm = null;
            }

            _runningTask = null; // Clear the reference to the completed task
        }
    }

    private void RunEvolutionWithControl(IGeneticAlgorithm algorithm, int generations, CancellationToken cancellationToken)
    {
        for (int generation = 0; generation < generations; generation++)
        {
            // Check for cancellation
            if (cancellationToken.IsCancellationRequested)
            {
                break; // Exit the loop if cancellation is requested
            }

            // Update Snap Icon and Availability
            Application.Current.Dispatcher.Invoke(() =>  
            {
                (ThanosSnapCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(ThanosSnapTriggered));
            });
        

            // Wait if paused
            _pauseEvent.Wait(cancellationToken);

            // Run one generation
            algorithm.RunEvolution(1, cancellationToken);
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
        if (IsAlgorithmRunning)
        {
            _cancellationTokenSource?.Cancel(); // Cancel the running algorithm
            _pauseEvent.Set(); // Ensure the algorithm is not paused, so it can exit immediately
            IsAlgorithmRunning = false; // Reset the state
            IsPaused = false; // Reset the paused state
        }
    }

    private void ExecuteThanosSnap()
    {
        if (_algorithm is null)
            return;

        var result = MessageBox.Show(
            "Are you sure you want to perform the Thanos Snap? This will cut the population in half at the end of the current generation.",
            "Confirm Thanos Snap",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _algorithm.ThanosSnapTriggered = true;
            (ThanosSnapCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            OnPropertyChanged(nameof(ThanosSnapTriggered));
        }
    }

    private async Task StopRunningTaskAsync()
    {
        if (_runningTask is not null && !_runningTask.IsCompleted)
        {
            _cancellationTokenSource?.Cancel(); // Signal cancellation
            try
            {
                await _runningTask; // Wait for the task to complete
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation exceptions
            }
        }
    }

    private IGeneticAlgorithm GetAlgorithm()
    {
        string? filePath = null;
        if (Parameters.OutputDirectory != null)
            filePath = Path.Combine(Parameters.OutputDirectory!, "RunParameters.json");

        IGeneticAlgorithm algorithm = null!;
        switch (Parameters.Parameters)
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
            case ConnectFourGeneticAlgorithmParameters conn:
                algorithm = new ConnectFourGeneticAlgorithm(conn);
                if (Parameters.OutputDirectory != null)
                    conn.SaveToFile(filePath);
                break;
        }

        return algorithm;
    }

    #endregion

    #region Algorithm Type and Parameter Selection

    private bool _useChromosomeTemplate;
    private GeneticAlgorithmType _selectedAlgorithmType = GeneticAlgorithmType.Quadratic;
    private ChromosomeViewModel _chromosomeViewModel;
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
                Parameters = SelectedAlgorithmType.ToNewViewModel();

                // Update the Chromosome Template to be the correct type
                ChromosomeTemplate = SelectedAlgorithmType.ToNewChromosomeViewModel();
            }
        }
    }


    private GeneticAlgorithmParametersViewModel _parameters;
    public GeneticAlgorithmParametersViewModel Parameters
    {
        get => _parameters;
        set
        {
            _parameters = value;
            OnPropertyChanged(nameof(Parameters));

            _selectedAlgorithmType = Parameters.Parameters.FromParams();
            SelectedAlgorithmType = _selectedAlgorithmType;
            OnPropertyChanged(nameof(SelectedAlgorithmType));

            // Update the Chromosome Template to be the correct type
            if (value.Parameters.TemplateChromosome != null)
            {
                ChromosomeTemplate = new ChromosomeViewModel(value.Parameters.TemplateChromosome);
                UseChromosomeTemplate = true;
            }
            else
            {
                ChromosomeTemplate = SelectedAlgorithmType.ToNewChromosomeViewModel();
                UseChromosomeTemplate = false;
            }
        }
    }

    public ChromosomeViewModel ChromosomeTemplate
    {
        get => _chromosomeViewModel;
        set
        {
            _chromosomeViewModel = value;
            OnPropertyChanged(nameof(ChromosomeTemplate));
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
                Parameters.Parameters.TemplateChromosome = ChromosomeTemplate.BaseChromosome;
            }
            else
            {
                Parameters.Parameters.TemplateChromosome = null;
            }
        }
    }



    #endregion

    #region Plotting

    public PlotTabControlViewModel TopPlotsTabControl { get; } = new();
    public PlotTabControlViewModel BottomPlotsTabControl { get; } = new();

    private void SetUpPlots()
    {
        TopPlotsTabControl.SetUpPlots(Parameters.Parameters, ChromosomeTemplate.BaseChromosome);
        BottomPlotsTabControl.SetUpPlots(Parameters.Parameters, ChromosomeTemplate.BaseChromosome);
    }

    private void OnGenerationFinished(int generation, GenerationLog generationLog)
    {
        CurrentGeneration = generation;
        _generationalLogs.Enqueue(generationLog);

        var sortedLogs = _generationalLogs.OrderBy(p => p.Generation)
            .ToList();
        TopPlotsTabControl.UpdatePlots(sortedLogs);
        BottomPlotsTabControl.UpdatePlots(sortedLogs);
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
                    Parameters = GeneticAlgorithmParameters.LoadFromFile(filePath).ToViewModel();
                    OnPropertyChanged(nameof(Parameters));
                    SetUpPlots();

                    var previousGenerationLogs = GetAlgorithm().Logger.ReadGenerationLogs();
                    if (previousGenerationLogs.Count == 0)
                        return;

                    for (int i = 0; i < previousGenerationLogs.Count -1; i++)
                    {
                        _generationalLogs.Enqueue(previousGenerationLogs[i]);   
                    }
                    OnGenerationFinished(previousGenerationLogs[^1].Generation, previousGenerationLogs[^1]);
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

public class PlotTabControlViewModel : BaseViewModel
{
    public GeneticAlgorithmParameters Parameters { get; set; }
    public PlotManager FitnessPlotManager { get; set; }
    public PlotManager AverageStatPlotManager { get; set; }
    public PlotManager BestChromosomeHeatmapPlotManager { get; set; }
    public PlotManager AverageChromosomeHeatmapPlotManager { get; set; }

    public PlotTabControlViewModel()
    {
        FitnessPlotManager = new PlotManager(new WpfPlot(), new FitnessPlotStrategy());
        AverageStatPlotManager = new PlotManager(new WpfPlot(), new AverageStatLinePlotStrategy());
        BestChromosomeHeatmapPlotManager = new PlotManager(new WpfPlot(), new GenerationStatHeatmapStrategy());
        AverageChromosomeHeatmapPlotManager = new PlotManager(new WpfPlot(), new GenerationStatHeatmapStrategy());
    }

    public void SetUpPlots(GeneticAlgorithmParameters parameters, Chromosome template)
    {
        Parameters = parameters;
        FitnessPlotManager.PlottingStrategy = new FitnessPlotStrategy() { Generations = parameters.Generations };
        FitnessPlotManager.SetUpPlot();

        AverageStatPlotManager.PlottingStrategy = new AverageStatLinePlotStrategy() { Generations = parameters.Generations };
        AverageStatPlotManager.SetUpPlot();

        Dictionary<string, bool> statNames = template.MutableStatsByName.Keys.Reverse().ToDictionary(kvp => kvp, _ => true);
        BestChromosomeHeatmapPlotManager.PlottingStrategy = new GenerationStatHeatmapStrategy() { Generations = parameters.Generations, StatVisibility = statNames, PlotAverage = false };
        BestChromosomeHeatmapPlotManager.SetUpPlot();
        AverageChromosomeHeatmapPlotManager.PlottingStrategy = new GenerationStatHeatmapStrategy() { Generations = parameters.Generations, StatVisibility = statNames, PlotAverage = true };
        AverageChromosomeHeatmapPlotManager.SetUpPlot();
    }

    public void UpdatePlots(List<GenerationLog> sortedGenerationLogs)
    {
        FitnessPlotManager.UpdatePlot(sortedGenerationLogs);
        AverageStatPlotManager.UpdatePlot(sortedGenerationLogs);
        BestChromosomeHeatmapPlotManager.UpdatePlot(sortedGenerationLogs);
        AverageChromosomeHeatmapPlotManager.UpdatePlot(sortedGenerationLogs);

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

            lock (AverageStatPlotManager.Plot.Plot.Sync)
            {
                AverageStatPlotManager.Plot.Plot.Add.Plottable(aLine);
                AverageStatPlotManager.Plot.Plot.Add.Plottable(bLine);
                AverageStatPlotManager.Plot.Plot.Add.Plottable(cLine);
                AverageStatPlotManager.Plot.Plot.Add.Plottable(yIntLine);
            }
            AverageStatPlotManager.Plot.Refresh();
        }
    }

}