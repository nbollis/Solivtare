using System.Text.Json;

namespace SolvitaireGenetics;

public class QuadraticGeneticAlgorithmParameters : GeneticAlgorithmParameters
{
    public double CorrectA { get; set; } = 0.46;
    public double CorrectB { get; set; } = 0.67;
    public double CorrectC { get; set; } = -0.74;
    public double CorrectIntercept { get; set; } = -1.68;

    public override void SaveToFile(string filePath)
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }
}