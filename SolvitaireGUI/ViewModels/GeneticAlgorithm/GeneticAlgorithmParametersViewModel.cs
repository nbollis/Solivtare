using System.Windows;
using System.Windows.Input;
using SolvitaireGenetics;
using SolvitaireGenetics.IO;

namespace SolvitaireGUI;

public abstract class GeneticAlgorithmParametersViewModel : BaseViewModel
{
    private GeneticAlgorithmParameters _parameters;

    public GeneticAlgorithmParameters Parameters
    {
        get => _parameters;
        set
        {
            _parameters = value;
            OnPropertyChanged(nameof(Parameters));
        }
    }

    public string? OutputDirectory
    {
        get => _parameters.OutputDirectory;
        set
        {
            _parameters.OutputDirectory = value;
            OnPropertyChanged(nameof(OutputDirectory));
        }
    }

    public LoggingType LoggingType
    {
        get => _parameters.LoggingType;
        set
        {
            _parameters.LoggingType = value;
            OnPropertyChanged(nameof(LoggingType));
        }
    }

    public SelectionStrategy SelectionStrategy
    {
        get => _parameters.SelectionStrategy;
        set
        {
            _parameters.SelectionStrategy = value;
            OnPropertyChanged(nameof(SelectionStrategy));
        }
    }

    public ReproductionStrategy ReproductionStrategy
    {
        get => _parameters.ReproductionStrategy;
        set
        {
            _parameters.ReproductionStrategy = value;
            OnPropertyChanged(nameof(ReproductionStrategy));
        }
    }

    public int PopulationSize
    {
        get => _parameters.PopulationSize;
        set
        {
            _parameters.PopulationSize = value;
            OnPropertyChanged(nameof(PopulationSize));
        }
    }

    public int Generations
    {
        get => _parameters.Generations;
        set
        {
            _parameters.Generations = value;
            OnPropertyChanged(nameof(Generations));
        }
    }

    public double MutationRate
    {
        get => _parameters.MutationRate;
        set
        {
            _parameters.MutationRate = value;
            OnPropertyChanged(nameof(MutationRate));
        }
    }

    public int TournamentSize
    {
        get => _parameters.TournamentSize;
        set
        {
            _parameters.TournamentSize = value;
            OnPropertyChanged(nameof(TournamentSize));
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

    public virtual ICommand SaveParametersCommand { get; }
    public virtual ICommand LoadParametersCommand { get; }
    public List<LoggingType> LoggingTypes { get; } 
    public List<ReproductionStrategy> ReproductionStrategies { get; }
    public List<SelectionStrategy> SelectionStrategies { get; }
    public GeneticAlgorithmParametersViewModel(GeneticAlgorithmParameters parameters)
    {
        _parameters = parameters;

        LoggingTypes = Enum.GetValues(typeof(LoggingType)).Cast<LoggingType>().ToList();
        ReproductionStrategies = Enum.GetValues(typeof(ReproductionStrategy)).Cast<ReproductionStrategy>().ToList();
        SelectionStrategies = Enum.GetValues(typeof(SelectionStrategy)).Cast<SelectionStrategy>().ToList();

        SaveParametersCommand = new RelayCommand(SaveParameters);
        LoadParametersCommand = new RelayCommand(LoadParameters);
    }

    private void SaveParameters()
    {
        try
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                Parameters.SaveToFile(saveFileDialog.FileName);
                MessageBox.Show("Parameters saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save parameters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected virtual void LoadParameters()
    {
        try
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Parameters = GeneticAlgorithmParameters.LoadFromFile(openFileDialog.FileName);
                OnPropertyChanged(null); // Notify all properties have changed
                MessageBox.Show("Parameters loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load parameters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}