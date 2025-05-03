namespace SolvitaireGenetics;

public class QuadraticGeneticAlgorithmParameters : GeneticAlgorithmParameters
{
    public double CorrectA { get; set; } = 3.33;
    public double CorrectB { get; set; } = 2.76;
    public double CorrectC { get; set; } = 1.74;
    public double CorrectIntercept { get; set; } = 3.2;

    public override void SaveToFile(string filePath)
    {
        // Implement your save logic here
    }
}