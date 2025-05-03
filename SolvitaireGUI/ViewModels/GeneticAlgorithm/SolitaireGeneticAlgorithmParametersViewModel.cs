using System.Windows;
using SolvitaireCore;
using SolvitaireGenetics;

namespace SolvitaireGUI;

public class SolitaireGeneticAlgorithmParametersViewModel(SolitaireGeneticAlgorithmParameters parameters)
    : GeneticAlgorithmParametersViewModel(parameters)
{
    public string? DecksToUse
    {
        get => ((SolitaireGeneticAlgorithmParameters)_parameters).DecksToUse;
        set
        {
            ((SolitaireGeneticAlgorithmParameters)_parameters).DecksToUse = value;
            OnPropertyChanged(nameof(DecksToUse));
        }
    }

    public int MaxMovesPerGeneration
    {
        get => ((SolitaireGeneticAlgorithmParameters)_parameters).MaxMovesPerGeneration;
        set
        {
            ((SolitaireGeneticAlgorithmParameters)_parameters).MaxMovesPerGeneration = value;
            OnPropertyChanged(nameof(MaxMovesPerGeneration));
        }
    }

    public int MaxGamesPerGeneration
    {
        get => ((SolitaireGeneticAlgorithmParameters)_parameters).MaxGamesPerGeneration;
        set
        {
            ((SolitaireGeneticAlgorithmParameters)_parameters).MaxGamesPerGeneration = value;
            OnPropertyChanged(nameof(MaxGamesPerGeneration));
        }
    }

    public SolitaireGeneticAlgorithmParameters GetParameters()
    {
        return (SolitaireGeneticAlgorithmParameters)_parameters;
    }

    protected override void LoadParameters()
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

public class SolitaireGeneticAlgorithmParametersModel : SolitaireGeneticAlgorithmParametersViewModel
{
    public static SolitaireGeneticAlgorithmParametersModel Instance => new();

    SolitaireGeneticAlgorithmParametersModel() : base(new SolitaireGeneticAlgorithmParameters())
    {

    }
}