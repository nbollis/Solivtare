namespace SolvitaireCore;

public class TicTacToeGameState : BaseGameState<TicTacToeMove>,
    ITwoPlayerGameState<TicTacToeMove>, IEquatable<TicTacToeGameState>
{
    public const int Size = 3;
    public int[,] Board { get; private set; } = new int[Size, Size]; // 0 = empty, 1 = player1, 2 = player2
    public int CurrentPlayer { get; private set; } = 1;
    public int? WinningPlayer { get; private set; } = null;
    public override bool IsGameWon { get; protected set; }
    public bool IsGameDraw => !IsGameWon && MovesMade == Size * Size;
    public override bool IsGameLost => false; // Not typically used in TicTacToe
    public List<(int Row, int Col)> WinningCells { get; } = new();

    protected override void ResetInternal()
    {
        Board = new int[Size, Size];
        CurrentPlayer = 1;
        IsGameWon = false;
        WinningPlayer = null;
        WinningCells.Clear();
    }

    protected override List<TicTacToeMove> GenerateLegalMoves()
    {
        var moves = new List<TicTacToeMove>();
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
                if (Board[row, col] == 0)
                    moves.Add(new TicTacToeMove(row, col));
        return moves;
    }

    protected override void ExecuteMoveInternal(TicTacToeMove move)
    {
        if (Board[move.Row, move.Col] != 0)
            throw new InvalidOperationException("Cell already occupied.");

        Board[move.Row, move.Col] = CurrentPlayer;

        if (CheckWin(move.Row, move.Col))
        {
            IsGameWon = true;
            WinningPlayer = CurrentPlayer;
        }
        CurrentPlayer = 3 - CurrentPlayer; // Toggle between 1 and 2
    }

    protected override void UndoMoveInternal(TicTacToeMove move)
    {
        if (Board[move.Row, move.Col] == 0)
            throw new InvalidOperationException("Cell already empty.");

        Board[move.Row, move.Col] = 0;
        CurrentPlayer = 3 - CurrentPlayer;
        IsGameWon = false;
        WinningPlayer = null;
        WinningCells.Clear();
    }

    protected override IGameState<TicTacToeMove> CloneInternal()
    {
        var clone = new TicTacToeGameState
        {
            Board = (int[,])Board.Clone(),
            CurrentPlayer = CurrentPlayer,
            MovesMade = MovesMade,
            IsGameWon = IsGameWon,
            WinningPlayer = WinningPlayer
        };
        clone.WinningCells.AddRange(WinningCells);
        return clone;
    }

    private bool CheckWin(int row, int col)
    {
        int player = Board[row, col];
        // Check row
        if (Enumerable.Range(0, Size).All(c => Board[row, c] == player))
        {
            WinningCells.Clear();
            for (int c = 0; c < Size; c++) WinningCells.Add((row, c));
            return true;
        }
        // Check column
        if (Enumerable.Range(0, Size).All(r => Board[r, col] == player))
        {
            WinningCells.Clear();
            for (int r = 0; r < Size; r++) WinningCells.Add((r, col));
            return true;
        }
        // Check main diagonal
        if (row == col && Enumerable.Range(0, Size).All(i => Board[i, i] == player))
        {
            WinningCells.Clear();
            for (int i = 0; i < Size; i++) WinningCells.Add((i, i));
            return true;
        }
        // Check anti-diagonal
        if (row + col == Size - 1 && Enumerable.Range(0, Size).All(i => Board[i, Size - 1 - i] == player))
        {
            WinningCells.Clear();
            for (int i = 0; i < Size; i++) WinningCells.Add((i, Size - 1 - i));
            return true;
        }
        return false;
    }

    public bool IsPlayerWin(int player) => IsGameWon && WinningPlayer == player;
    public bool IsPlayerLoss(int player) => IsGameWon && WinningPlayer != player;

    public bool Equals(TicTacToeGameState? other)
    {
        if (other == null) return false;
        if (CurrentPlayer != other.CurrentPlayer || MovesMade != other.MovesMade) return false;
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                if (Board[r, c] != other.Board[r, c])
                    return false;
        return true;
    }

    protected override int GenerateHashCode()
    {
        int hash = 17;
        foreach (var cell in Board)
            hash = hash * 31 + cell.GetHashCode();
        hash = hash * 31 + CurrentPlayer;
        return hash;
    }
}