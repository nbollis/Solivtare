namespace SolvitaireCore.ConnectFour;

public class ConnectFourMove(int column) : IMove<ConnectFourGameState>
{
    public bool IsTerminatingMove => false;
    public int Column { get; } = column;

    public bool IsValid(ConnectFourGameState gameState)
    {
        // Top slot of the column must be empty
        return gameState.Board[0, Column] == 0;
    }

    public override string ToString() => $"Drop in column {Column}";
}