using SolvitaireCore;

namespace SolvitaireIO;

public static class SolitaireGameStateMapper
{
    public static SolitaireGameStateDto ToDTO(SolitaireGameState gameState)
    {
        return new SolitaireGameStateDto
        {
            CardsPerCycle = gameState.CardsPerCycle,
            MaximumCycles = gameState.MaximumCycles,
            CycleCount = gameState.CycleCount,
            MovesMade = gameState.MovesMade,
            TableauPiles = gameState.TableauPiles,
            FoundationPiles = gameState.FoundationPiles,
            StockPile = gameState.StockPile,
            WastePile = gameState.WastePile
        };
    }

    public static SolitaireGameState FromDTO(SolitaireGameStateDto dto)
    {
        var gameState = new SolitaireGameState(dto.CardsPerCycle)
        {
            CycleCount = dto.CycleCount,
            MovesMade = dto.MovesMade
        };

        gameState.TableauPiles = dto.TableauPiles;
        gameState.FoundationPiles = dto.FoundationPiles;
        gameState.StockPile = dto.StockPile;
        gameState.WastePile = dto.WastePile;

        return gameState;
    }
}