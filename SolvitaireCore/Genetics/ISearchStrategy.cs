namespace SolvitaireCore;

/// <summary>
/// Encodes the logic for how an agent makes a decision. 
/// </summary>
/// <typeparam name="TGameState"></typeparam>
/// <typeparam name="TMove"></typeparam>
public interface ISearchStrategy<in TGameState, TMove>
    where TGameState : IGameState<TMove>
    where TMove : IMove<TGameState>
{
    string Name { get; }
    AgentDecision GetNextAction(TGameState gameState, IReadOnlyList<Chromosome> chromosomes);
}

/// <summary>
/// A search strategy that randomly selects a move from the list of legal moves.
/// </summary>
public class RandomSearchStrategy<TGameState> : ISearchStrategy<TGameState>
    where TGameState : IGameState
{
    private readonly Random _random = new();
    public string Name => "Random";

    public AgentDecision GetNextAction(IGameState gameState, IReadOnlyList<Chromosome> chromosomes)
    {
        var moves = gameState.GetLegalMoves();
        if (moves.Count == 0)
            return AgentDecision.SkipGame();

        // Select a random move from the list of legal moves
        var move = moves[_random.Next(moves.Count)];
        return AgentDecision.PlayMove(move);
    }
}

/// <summary>
/// A search strategy that uses the MaxiMax algorithm to evaluate and select the best move.
/// </summary>
public class MaxiMaxSearchStrategy 
{

}