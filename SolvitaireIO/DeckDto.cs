using SolvitaireCore;

namespace SolvitaireIO;

public class DeckDto
{
    public int Seed { get; set; }
    public int Shuffles { get; set; }
    public List<Card> Cards { get; set; }
}

public class MinimizedDeckDto
{
    public int Seed { get; set; }
    public int Shuffles { get; set; }
}