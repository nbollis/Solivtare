
namespace SolvitaireCore;

public class GameStateEventArgs(SolitaireGameState result) : EventArgs()
{
    public SolitaireGameState Result { get; set; } = result;

}

public class AgentSimulation
{
    public readonly BaseAgent<SolitaireGameState, SolitaireMove> Agent;
    public readonly StandardDeck Deck;

    public static event EventHandler<GameStateEventArgs> GameWonHandler = null!; 

    public AgentSimulation(BaseAgent<SolitaireGameState, SolitaireMove> agent, StandardDeck deck)
    {
        Agent = agent;
        Deck = deck;
    }

    public AgentSimulationResult RunAgentSimulation(SolitaireGameState gameState, CancellationToken cancellation)
    {
        int movesPlayed = 0;
        int gamesPlayed = 0;
        int gamesWon = 0;
        while (cancellation.IsCancellationRequested == false)
        {
            Deck.Shuffle();
            Agent.ResetState();
            gameState.Reset();
            gameState.DealCards(Deck);

            gamesPlayed++;
            while (!gameState.IsGameWon)
            {
                if (cancellation.IsCancellationRequested)
                {
                    break;
                }

                var decision = Agent.GetNextAction(gameState);
                if (decision.ShouldSkip)
                {
                    break;
                }
                else
                {
                    gameState.ExecuteMove(decision);
                }

                movesPlayed++;
            }

            if (gameState.IsGameWon)
            {
                gamesWon++;
            }
        }

        return new AgentSimulationResult(movesPlayed, gamesPlayed, gamesWon);
    }

    public void GameWon(SolitaireGameState gamestate)
    {
        GameWonHandler?.Invoke(this, new(gamestate.Clone()));
    }


}

public class AgentSimulationResult
{
    public readonly int MovesPlayed;
    public readonly int GamesPlayed;
    public readonly int GamesWon;

    public AgentSimulationResult(int movesPlayed, int gamesPlayed, int gamesWon)
    {
        MovesPlayed = movesPlayed;
        GamesPlayed = gamesPlayed;
        GamesWon = gamesWon;
    }

    public double WinRate => (double)GamesWon / GamesPlayed;
    public double MovesPerGame => (double)MovesPlayed / GamesPlayed;
    public double MovesPerWin => (double)MovesPlayed / GamesWon;
    public double WinRatePerMove => (double)GamesWon / MovesPlayed;
}

