using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using SolvitaireGenetics;

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

    public ICommand SaveParametersCommand { get; }
    public ICommand LoadParametersCommand { get; }

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

    protected abstract void LoadParameters();

    public static GeneticAlgorithmParametersViewModel LoadFromFile(string filePath)
    {
        var paramseters = GeneticAlgorithmParameters.LoadFromFile(filePath);

        return paramseters switch
        {
            SolitaireGeneticAlgorithmParameters solitaireParams => new SolitaireGeneticAlgorithmParametersViewModel(solitaireParams),
            _ => throw new NotSupportedException("Unknown parameter type in the configuration file.")
        };
    }
}