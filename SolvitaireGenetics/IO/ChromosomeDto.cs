namespace SolvitaireGenetics;

/// <summary>
/// DTO for representing a chromosome.
/// </summary>
public class ChromosomeDto
{
    public Dictionary<string, double> Weights { get; set; } = new();
}