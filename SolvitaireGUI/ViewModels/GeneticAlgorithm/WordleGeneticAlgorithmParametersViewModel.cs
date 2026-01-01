using System.Windows;
using SolvitaireGenetics;

namespace SolvitaireGUI;

public class WordleGeneticAlgorithmParametersViewModel(WordleGeneticAlgorithmParameters parameters)
    : GeneticAlgorithmParametersViewModel(parameters)
{
    public int GamesPerAgent
    {
        get => ((WordleGeneticAlgorithmParameters)Parameters).GamesPerAgent;
        set
        {
            ((WordleGeneticAlgorithmParameters)Parameters).GamesPerAgent = value;
            OnPropertyChanged(nameof(GamesPerAgent));
        }
    }

    public int MaxGuesses
    {
        get => ((WordleGeneticAlgorithmParameters)Parameters).MaxGuesses;
        set
        {
            ((WordleGeneticAlgorithmParameters)Parameters).MaxGuesses = value;
            OnPropertyChanged(nameof(MaxGuesses));
        }
    }

    public int WordLength
    {
        get => ((WordleGeneticAlgorithmParameters)Parameters).WordLength;
        set
        {
            ((WordleGeneticAlgorithmParameters)Parameters).WordLength = value;
            OnPropertyChanged(nameof(WordLength));
        }
    }

    public double SpeedBonus
    {
        get => ((WordleGeneticAlgorithmParameters)Parameters).SpeedBonus;
        set
        {
            ((WordleGeneticAlgorithmParameters)Parameters).SpeedBonus = value;
            OnPropertyChanged(nameof(SpeedBonus));
        }
    }

    public double LossPenalty
    {
        get => ((WordleGeneticAlgorithmParameters)Parameters).LossPenalty;
        set
        {
            ((WordleGeneticAlgorithmParameters)Parameters).LossPenalty = value;
            OnPropertyChanged(nameof(LossPenalty));
        }
    }

    public string? FirstWordPoolFile
    {
        get => ((WordleGeneticAlgorithmParameters)Parameters).FirstWordPoolFile;
        set
        {
            ((WordleGeneticAlgorithmParameters)Parameters).FirstWordPoolFile = value;
            OnPropertyChanged(nameof(FirstWordPoolFile));
        }
    }

    public int FirstWordPoolCount => ((WordleGeneticAlgorithmParameters)Parameters).FirstWordPool.Count;

    public bool UseFixedTargetWords
    {
        get => ((WordleGeneticAlgorithmParameters)Parameters).UseFixedTargetWords;
        set
        {
            ((WordleGeneticAlgorithmParameters)Parameters).UseFixedTargetWords = value;
            OnPropertyChanged(nameof(UseFixedTargetWords));
        }
    }

    public WordleGeneticAlgorithmParameters GetParameters()
    {
        return (WordleGeneticAlgorithmParameters)Parameters;
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

public class WordleGeneticAlgorithmParametersModel : WordleGeneticAlgorithmParametersViewModel
{
    public static WordleGeneticAlgorithmParametersModel Instance => new();

    WordleGeneticAlgorithmParametersModel() : base(new WordleGeneticAlgorithmParameters())
    {
    }
}
