using SolvitaireIO;

namespace SolvitaireGenetics;

/// <summary>
/// DTO for logging agent-specific information.
/// </summary>
public class AgentLog
{
    public int Generation { get; set; }
    public double Fitness { get; set; }
    public int GamesWon { get; set; }
    public int MovesMade { get; set; }
    public int GamesPlayed { get; set; }
    public Chromosome Chromosome { get; set; } = null!;
}