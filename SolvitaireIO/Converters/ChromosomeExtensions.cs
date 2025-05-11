using System.Text;
using SolvitaireCore;

namespace SolvitaireIO;

public static class ChromosomeExtensions
{
    public static string ToGeneData(this Chromosome chromosome)
    {
        var sb = new StringBuilder();
        foreach (var kvp in chromosome.MutableStatsByName)
        {
            sb.Append($"{kvp.Key}:{kvp.Value},");
        }
        if (sb.Length > 0)
        {
            sb.Length--; // Remove the last comma
        }
        return sb.ToString();
    }

    public static string[] GetWeightNames(this string geneDataString)
    {
        var pairs = geneDataString.Split(',');
        var weightNames = new List<string>();
        foreach (var pair in pairs)
        {
            var kvp = pair.Split(':');
            if (kvp.Length == 2)
            {
                weightNames.Add(kvp[0]);
            }
        }
        return weightNames.ToArray();
    }

    public static void LoadGeneData(this Chromosome chromosome, string geneData)
    {
        var pairs = geneData.Split(',');
        foreach (var pair in pairs)
        {
            var kvp = pair.Split(':');
            if (kvp.Length == 2 && double.TryParse(kvp[1], out var value))
            {
                chromosome.MutableStatsByName[kvp[0]] = value;
            }
        }
    }
}