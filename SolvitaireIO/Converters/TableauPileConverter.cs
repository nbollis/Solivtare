using System.Text.Json;
using System.Text.Json.Serialization;
using SolvitaireCore;

namespace SolvitaireIO;
public class TableauPileConverter : JsonConverter<TableauPile>
{
    public override TableauPile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonObject = JsonDocument.ParseValue(ref reader).RootElement;
        int index = jsonObject.GetProperty("Index").GetInt32();
        var cards = JsonSerializer.Deserialize<List<Card>>(jsonObject.GetProperty("Cards").GetRawText(), options);
        return new TableauPile(index, cards);
    }

    public override void Write(Utf8JsonWriter writer, TableauPile value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, new
        {
            value.Index,
            value.Cards
        }, options);
    }
}

public class WastePileConverter : JsonConverter<WastePile>
{
    public override WastePile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonObject = JsonDocument.ParseValue(ref reader).RootElement;
        int index = jsonObject.GetProperty("Index").GetInt32();
        var cards = JsonSerializer.Deserialize<List<Card>>(jsonObject.GetProperty("Cards").GetRawText(), options);
        return new WastePile(index, cards);
    }

    public override void Write(Utf8JsonWriter writer, WastePile value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, new
        {
            value.Index,
            value.Cards
        }, options);
    }
}

public class StockPileConverter : JsonConverter<StockPile>
{
    public override StockPile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonObject = JsonDocument.ParseValue(ref reader).RootElement;
        int index = jsonObject.GetProperty("Index").GetInt32();
        var cards = JsonSerializer.Deserialize<List<Card>>(jsonObject.GetProperty("Cards").GetRawText(), options);
        return new StockPile(index, cards);
    }

    public override void Write(Utf8JsonWriter writer, StockPile value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, new
        {
            value.Index,
            value.Cards
        }, options);
    }
}
