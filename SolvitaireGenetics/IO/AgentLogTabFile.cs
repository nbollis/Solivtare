using System.Globalization;
using SolvitaireCore;
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


    /// <summary>
    /// Reads a tab-separated file into a list of AgentLog objects.
    /// </summary>
    /// <param name="filePath">The file path to read from.</param>
    /// <returns>A list of AgentLog objects.</returns>
    public static List<AgentLog> ReadFromFile(string filePath)
    {
        var agentLogs = new List<AgentLog>();

        using var reader = new StreamReader(filePath);

        // Read the header
        var header = reader.ReadLine();
        if (header == null)
        {
            throw new InvalidDataException("The file does not have a valid header.");
        }

        var headers = header.Split('\t');
        if (headers.Length < 7 || headers[6] != "ChromosomeType")
        {
            throw new InvalidDataException("The file does not have a valid header.");
        }

        // Extract weight names from the headers
        var weightNames = headers.Skip(7).ToList();

        // Read each line
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var parts = line.Split('\t');
            if (parts.Length != headers.Length)
            {
                throw new InvalidDataException("The file contains an invalid row.");
            }

            // Create the chromosome
            var typeName = parts[6];
            var type = Type.GetType(typeName);
            if (type == null || !typeof(Chromosome).IsAssignableFrom(type))
            {
                throw new InvalidDataException($"Unknown or invalid chromosome type: {typeName}");
            }

            var chromosome = (Chromosome)Activator.CreateInstance(type)!;

            // Populate the weights
            for (int i = 0; i < weightNames.Count; i++)
            {
                var weightName = weightNames[i];
                if (!string.IsNullOrEmpty(parts[7 + i]))
                {
                    chromosome.SetWeight(weightName, double.Parse(parts[7 + i], CultureInfo.InvariantCulture));
                }
            }

            // Create the AgentLog
            var agentLog = new AgentLog
            {
                Generation = int.Parse(parts[0], CultureInfo.InvariantCulture),
                Count = float.Parse(parts[1], CultureInfo.InvariantCulture),
                Fitness = double.Parse(parts[2], CultureInfo.InvariantCulture),
                GamesWon = int.Parse(parts[3], CultureInfo.InvariantCulture),
                MovesMade = int.Parse(parts[4], CultureInfo.InvariantCulture),
                GamesPlayed = int.Parse(parts[5], CultureInfo.InvariantCulture),
                Chromosome = ChromosomeLog.FromChromosome(chromosome)
            };

            agentLog.Chromosome.Fitness = agentLog.Fitness;
            agentLogs.Add(agentLog);
        }

        return agentLogs;
    }
}
