using System.Globalization;
using SolvitaireIO.Database.Models;

namespace SolvitaireGenetics;

/// <summary>  
/// Utility class for handling tab-separated files of AgentLog objects.  
/// </summary>  
public static class AgentLogTabFile
{
    private static readonly string[] Headers =
    {
       "Generation", "Count", "Fitness", "GamesWon", "MovesMade", "GamesPlayed", "ChromosomeType"
   };

    /// <summary>  
    /// Writes a list of AgentLog objects to a tab-separated file.  
    /// </summary>  
    /// <param name="filePath">The file path to write to.</param>  
    /// <param name="agentLogs">The list of AgentLog objects to write.</param>  
    public static void WriteToFile(string filePath, List<AgentLog> agentLogs)
    {
        using var writer = new StreamWriter(filePath);

        // Collect all unique weight names from the chromosomes  
        var weightNames = agentLogs
           .SelectMany(log => log.Chromosome.GeneData.Split(",").Select(p => p.Split(':')[0]))
           .Distinct()
           .OrderBy(name => name)
           .ToList();

        // Generate the headers  
        var headers = Headers.Concat(weightNames).ToList();

        // Write the header  
        writer.WriteLine(string.Join("\t", headers));

        // Write each AgentLog  
        foreach (var log in agentLogs)
        {
            var row = new List<string>
              {
                  log.Generation.ToString(CultureInfo.InvariantCulture),
                  log.Count.ToString(CultureInfo.InvariantCulture),
                  log.Fitness.ToString(CultureInfo.InvariantCulture),
                  log.GamesWon.ToString(CultureInfo.InvariantCulture),
                  log.MovesMade.ToString(CultureInfo.InvariantCulture),
                  log.GamesPlayed.ToString(CultureInfo.InvariantCulture),
                  log.Chromosome.ChromosomeType
              };

            // Parse the gene data directly to extract weights  
            var geneData = log.Chromosome.GeneData.Split(",")
                .Select(p => p.Split(':'))
                .ToDictionary(parts => parts[0], parts => parts.Length > 1 ? parts[1] : string.Empty);

            // Add the weights in the order of the headers  
            row.AddRange(weightNames.Select(weightName =>
                geneData.TryGetValue(weightName, out var value)
                    ? value
                    : string.Empty));

            writer.WriteLine(string.Join("\t", row));
        }
    }
}
