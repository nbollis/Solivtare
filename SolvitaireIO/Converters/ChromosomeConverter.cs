using System.Text.Json;
using System.Text.Json.Serialization;
using SolvitaireCore;

namespace SolvitaireIO;

public class ChromosomeConverter<TChromosome> : JsonConverter<Chromosome> 
    where TChromosome : Chromosome, new()
{
    public override Chromosome Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonObject = JsonDocument.ParseValue(ref reader).RootElement;
        var mutableStats = JsonSerializer.Deserialize<Dictionary<string, double>>(jsonObject.GetProperty("MutableStatsByName").GetRawText(), options);

        var chromosome = (TChromosome)Activator.CreateInstance(typeof(TChromosome), Random.Shared)!;
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
            value.MutableStatsByName,
        }, options);
    }
}