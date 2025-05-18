namespace SolvitaireCore;

public class RandomSolitaireAgent : RandomAgent<SolitaireGameState, SolitaireMove>
{
    private readonly Random _random = new Random();

    public override SolitaireMove GetNextAction(SolitaireGameState gameState)
    {
        var moves = gameState.GetLegalMoves();
        if (moves.Count == 0)
            return new SkipGameMove();

        var move = moves[_random.Next(moves.Count)];
        return move;
    }
}