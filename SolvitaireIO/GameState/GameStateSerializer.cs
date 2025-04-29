using System.Text.Json;
using SolvitaireCore;

namespace SolvitaireIO;

/// <summary>
/// Handles serialization and deserialization of SolitaireGameState.
/// </summary>
public static class GameStateSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true, // Makes the JSON output more readable
        PropertyNameCaseInsensitive = true,
        Converters = { new TableauPileConverter(), new FoundationPileConverter(), new StockPileConverter(), new WastePileConverter() }
    };

    /// <summary>
    /// Serializes a SolitaireGameState to a JSON string.
    /// </summary>
    /// <param name="gameState">The game state to serialize.</param>
    /// <returns>A JSON string representing the game state.</returns>
    public static string Serialize(SolitaireGameState gameState)
    {
        var dto = SolitaireGameStateMapper.ToDTO(gameState);
        return JsonSerializer.Serialize(dto, SerializerOptions);
    }

    /// <summary>
    /// Deserializes a JSON string into a SolitaireGameState object.
    /// </summary>
    /// <param name="json">The JSON string representing the game state.</param>
    /// <returns>A SolitaireGameState object.</returns>
    public static SolitaireGameState Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<SolitaireGameStateDto>(json, SerializerOptions);
        if (dto == null) throw new InvalidOperationException("Failed to deserialize JSON.");
        return SolitaireGameStateMapper.FromDTO(dto);
    }
}