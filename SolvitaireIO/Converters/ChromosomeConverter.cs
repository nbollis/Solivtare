using System.Text.Json;
using System.Text.Json.Serialization;
using SolvitaireCore;
using SolvitaireIO.Database.Models;

namespace SolvitaireIO;

public class ChromosomeLogConverter : JsonConverter<ChromosomeLog> 
{

    public override ChromosomeLog Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonObject = JsonDocument.ParseValue(ref reader).RootElement;
        var fitness = jsonObject.GetProperty("Fitness").GetDouble();
        var geneData = jsonObject.GetProperty("GeneData").GetString()!;
        var chromosomeType = jsonObject.GetProperty("ChromosomeType").GetString()!;
        var speciesIdentifier = jsonObject.GetProperty("SpeciesIdentifier").GetString()!;
        var chromosome = (Chromosome)Activator.CreateInstance(Type.GetType(chromosomeType)!, Random.Shared)!;
        chromosome.Fitness = fitness;
        chromosome.SpeciesIndex = int.Parse(speciesIdentifier);
        chromosome.LoadGeneData(geneData);

        return new ChromosomeLog
        {
            StableHash = chromosome.GetStableHash(),
            ChromosomeType = chromosomeType,
            GeneData = geneData,
            Fitness = fitness,
            SpeciesIdentifier = speciesIdentifier,
        };
    }
    public override void Write(Utf8JsonWriter writer, ChromosomeLog value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, new
        {
            value.Fitness,
            value.GeneData,
            value.ChromosomeType,
            value.SpeciesIdentifier
        }, options);
    }
}

public class ChromosomeConverter<TChromosome> : JsonConverter<Chromosome> 
    where TChromosome : Chromosome, new()
{
    public override Chromosome Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonObject = JsonDocument.ParseValue(ref reader).RootElement;
        var fitness = jsonObject.GetProperty("Fitness").GetDouble();
        var mutableStats = JsonSerializer.Deserialize<Dictionary<string, double>>(jsonObject.GetProperty("MutableStatsByName").GetRawText(), options);

        var chromosome = (TChromosome)Activator.CreateInstance(typeof(TChromosome), Random.Shared)!;
        chromosome.Fitness = fitness;
        foreach (var kvp in mutableStats!)
        {
            chromosome.MutableStatsByName[kvp.Key] = kvp.Value;
        }
        return chromosome;
    }

    public override void Write(Utf8JsonWriter writer, Chromosome value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, new
        {
            value.Fitness,
            value.MutableStatsByName,
        }, options);
    }
}