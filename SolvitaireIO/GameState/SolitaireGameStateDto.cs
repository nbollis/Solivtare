using SolvitaireCore;

namespace SolvitaireIO;

public class SolitaireGameStateDto
{
    public int CardsPerCycle { get; set; }
    public int MaximumCycles { get; set; }
    public int CycleCount { get; set; }
    public int MovesMade { get; set; }
    public List<TableauPile> TableauPiles { get; set; } = new();
    public List<FoundationPile> FoundationPiles { get; set; } = new();
    public StockPile StockPile { get; set; } = new(SolitaireGameState.StockIndex);
    public WastePile WastePile { get; set; } = new(SolitaireGameState.WasteIndex);
}