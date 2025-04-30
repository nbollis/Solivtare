using System.Windows;
using System.Windows.Input;
using SolvitaireGenetics;

namespace SolvitaireGUI;

public class SolitaireGeneticAlgorithmParametersViewModel : BaseViewModel
{
    private SolitaireGeneticAlgorithmParameters _parameters;

    public string? OutputDirectory
    {
        get => _parameters.OutputDirectory;
        set
        {
            _parameters.OutputDirectory = value;
            OnPropertyChanged(nameof(OutputDirectory));
        }
    }

    public string? DecksToUse
    {
        get => _parameters.DecksToUse;
        set
        {
            _parameters.DecksToUse = value;
            OnPropertyChanged(nameof(DecksToUse));
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

    public int MaxMovesPerGeneration
    {
        get => _parameters.MaxMovesPerGeneration;
        set
        {
            _parameters.MaxMovesPerGeneration = value;
            OnPropertyChanged(nameof(MaxMovesPerGeneration));
        }
    }

    public int MaxGamesPerGeneration
    {
        get => _parameters.MaxGamesPerGeneration;
        set
        {
            _parameters.MaxGamesPerGeneration = value;
            OnPropertyChanged(nameof(MaxGamesPerGeneration));
        }
    }

    public ICommand SaveParametersCommand { get; }
    public ICommand LoadParametersCommand { get; }

    public SolitaireGeneticAlgorithmParametersViewModel()
    {
        _parameters = new SolitaireGeneticAlgorithmParameters();

        SaveParametersCommand = new RelayCommand(SaveParameters);
        LoadParametersCommand = new RelayCommand(LoadParameters);
    }

    public SolitaireGeneticAlgorithmParametersViewModel(SolitaireGeneticAlgorithmParameters parameters)
    {
        _parameters = parameters;

        SaveParametersCommand = new RelayCommand(SaveParameters);
        LoadParametersCommand = new RelayCommand(LoadParameters);
    }

    public SolitaireGeneticAlgorithmParameters GetParameters() => _parameters;

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

    private void LoadParameters()
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
                _parameters = SolitaireGeneticAlgorithmParameters.LoadFromFile(openFileDialog.FileName);
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