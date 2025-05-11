using System.Text.Json;
using System.Text.Json.Serialization;
using SolvitaireCore;

namespace SolvitaireIO;

public class FoundationPileConverter : JsonConverter<FoundationPile>
{
    public override FoundationPile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonObject = JsonDocument.ParseValue(ref reader).RootElement;
        int index = jsonObject.GetProperty("Index").GetInt32();
        Suit suit = (Suit)jsonObject.GetProperty("Suit").GetInt32();
        var cards = JsonSerializer.Deserialize<List<Card>>(jsonObject.GetProperty("Cards").GetRawText(), options);
        return new FoundationPile(suit, index, cards);
    }

    public override void Write(Utf8JsonWriter writer, FoundationPile value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, new
        {
            value.Index,
            value.Suit,
            value.Cards
        }, options);
    }
}