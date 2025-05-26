using System.Windows;
using SolvitaireGenetics;

namespace SolvitaireGUI;

public class SolitaireGeneticAlgorithmParametersViewModel(SolitaireGeneticAlgorithmParameters parameters)
    : GeneticAlgorithmParametersViewModel(parameters)
{
    public string? DecksToUse
    {
        get => ((SolitaireGeneticAlgorithmParameters)Parameters).DecksToUse;
        set
        {
            ((SolitaireGeneticAlgorithmParameters)Parameters).DecksToUse = value;
            OnPropertyChanged(nameof(DecksToUse));
        }
    }

    public int MaxMovesPerGeneration
    {
        get => ((SolitaireGeneticAlgorithmParameters)Parameters).MaxMovesPerGeneration;
        set
        {
            ((SolitaireGeneticAlgorithmParameters)Parameters).MaxMovesPerGeneration = value;
            OnPropertyChanged(nameof(MaxMovesPerGeneration));
        }
    }

    public int MaxGamesPerGeneration
    {
        get => ((SolitaireGeneticAlgorithmParameters)Parameters).MaxGamesPerGeneration;
        set
        {
            ((SolitaireGeneticAlgorithmParameters)Parameters).MaxGamesPerGeneration = value;
            OnPropertyChanged(nameof(MaxGamesPerGeneration));
        }
    }

    public SolitaireGeneticAlgorithmParameters GetParameters()
    {
        return (SolitaireGeneticAlgorithmParameters)Parameters;
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

public class SolitaireGeneticAlgorithmParametersModel : SolitaireGeneticAlgorithmParametersViewModel
{
    public static SolitaireGeneticAlgorithmParametersModel Instance => new();

    SolitaireGeneticAlgorithmParametersModel() : base(new SolitaireGeneticAlgorithmParameters())
    {

    }
}