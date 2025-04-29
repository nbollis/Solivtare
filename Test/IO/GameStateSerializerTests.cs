using System.Text.Json;
using NUnit.Framework;
using SolvitaireCore;
using SolvitaireIO;

namespace Test.IO;

[TestFixture]
public class GameStateSerializerTests
{
    [Test]
    public void Serialize_ValidGameState_ShouldReturnJsonString()
    {
        // Arrange  
        var gameState = new SolitaireGameState();
        gameState.DealCards(new StandardDeck());

        // Act  
        var json = GameStateSerializer.Serialize(gameState);

        // Assert  
        Assert.That(json, Is.Not.Null.And.Not.Empty);
        Assert.That(() => JsonDocument.Parse(json), Throws.Nothing);
    }

    [Test]
    public void Deserialize_ValidJsonString_ShouldReturnGameState()
    {
        // Arrange  
        var gameState = new SolitaireGameState();
        gameState.DealCards(new StandardDeck());
        var json = GameStateSerializer.Serialize(gameState);

        // Act  
        var deserializedGameState = GameStateSerializer.Deserialize(json);

        // Assert  
        Assert.That(deserializedGameState, Is.Not.Null);
        Assert.That(deserializedGameState.TableauPiles.Count, Is.EqualTo(gameState.TableauPiles.Count));
        Assert.That(deserializedGameState.FoundationPiles.Count, Is.EqualTo(gameState.FoundationPiles.Count));
    }

    [Test]
    public void Serialize_Deserialize_ShouldReturnEquivalentGameState()
    {
        // Arrange  
        var originalGameState = new SolitaireGameState();
        originalGameState.DealCards(new StandardDeck());

        // Act  
        var json = GameStateSerializer.Serialize(originalGameState);
        var deserializedGameState = GameStateSerializer.Deserialize(json);

        // Assert  
        Assert.That(deserializedGameState, Is.Not.Null);
        Assert.That(deserializedGameState, Is.EqualTo(originalGameState));
    }
}
