using System.Windows;
using System.Windows.Input;
using SolvitaireGenetics;
using SolvitaireGenetics.IO;

namespace SolvitaireGUI;

public abstract class GeneticAlgorithmParametersViewModel : BaseViewModel
{
    protected GeneticAlgorithmParameters _parameters;

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

    public virtual ICommand SaveParametersCommand { get; }
    public virtual ICommand LoadParametersCommand { get; }

    public GeneticAlgorithmParametersViewModel(GeneticAlgorithmParameters parameters)
    {
        _parameters = parameters;
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
                _parameters.SaveToFile(saveFileDialog.FileName);
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
                _parameters = GeneticAlgorithmParameters.LoadFromFile(openFileDialog.FileName);
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