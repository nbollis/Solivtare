namespace SolvitaireCore;

public class SolitaireGameEngine : IGameEngine
{
    private GameState _state;
    private StandardDeck _deck;

    public SolitaireGameEngine()
    {
        _state = new GameState();
        _deck = new StandardDeck();
    }

    public void PlayGame(IPlayer player)
    {
        _deck.Shuffle();
        _state.DealCards(_deck);
        while (!_state.IsGameWon)
        {
            IMove move = player.GetNextMove(_state);
            if (move.IsValid(_state))
            {
                move.Execute(_state);
            }
            else
            {
                // Optionally break loop if invalid or no move (depends on player type)
                //break;
            }
        }
        Console.WriteLine("Congratulations! You've won the game!");
    }
}