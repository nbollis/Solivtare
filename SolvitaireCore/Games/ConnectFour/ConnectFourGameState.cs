namespace SolvitaireCore.ConnectFour;

public class ConnectFourGameState : BaseGameState<ConnectFourMove>, 
    ITwoPlayerGameState<ConnectFourMove>, IEquatable<ConnectFourGameState>
{
    public const int Rows = 6;
    public const int Columns = 7; 
    private static readonly (int dRow, int dCol)[] Directions = [(1, 0), (0, 1), (1, 1), (1, -1)];

    // Cacheing to reduce redundant calculations. 
    private bool _cachedIsGameWon = false;
    private bool _cachedIsGameDraw = false;
    public (int Row, int Col)? LastMove = null;
    private int? _cachedWinningPlayer = null;
    private readonly List<(int Row, int Col)> _cachedWinningCells = new();
    private int[] _topRow = Enumerable.Repeat(Rows - 1, Columns).ToArray();

    public override bool IsGameWon => _cachedIsGameWon;
    public bool IsGameDraw => _cachedIsGameDraw;
    public override bool IsGameLost => false; // Implement if you want to track explicit losses
    public int? WinningPlayer => _cachedWinningPlayer;
    public IReadOnlyList<(int Row, int Col)> WinningCells => _cachedWinningCells;


    // 0 = empty, 1 = player1, 2 = player2
    public int[,] Board { get; private set; } = new int[Rows, Columns];
    public int CurrentPlayer { get; private set; } = 1;
    public bool BoardFull
    {
        get
        {
            for (var i = 0; i < _topRow.Length; i++)
            {
                var p = _topRow[i];
                if (p >= 0)
                    return false;
            }

            return true;
        }
    }
    public bool IsPlayerWin(int player) => IsGameWon && WinningPlayer == player;
    public bool IsPlayerLoss(int player) => IsGameWon && WinningPlayer != player;

    protected override void ExecuteMoveInternal(ConnectFourMove move)
    {
        int row = _topRow[move.Column];
        if (row < 0)
            throw new InvalidOperationException("Invalid move: column full");
        Board[row, move.Column] = CurrentPlayer;

        LastMove = (row, move.Column);
        CurrentPlayer = 3 - CurrentPlayer;
        _topRow[move.Column]--;
        UpdateWinAndDrawCache();
    }

    protected override void UndoMoveInternal(ConnectFourMove move)
    {
        int row = _topRow[move.Column] + 1;
        if (row >= Rows || Board[row, move.Column] == 0)
            throw new InvalidOperationException("Invalid undo: column empty");
        Board[row, move.Column] = 0;
        CurrentPlayer = 3 - CurrentPlayer;

        _topRow[move.Column]++;
        LastMove = null;
        UpdateWinAndDrawCache();
    }

    protected override void ResetInternal()
    {
        Board = new int[Rows, Columns];
        CurrentPlayer = 1;
        MovesMade = 0;
        _topRow = Enumerable.Repeat(Rows - 1, Columns).ToArray();
        LastMove = null;
        UpdateWinAndDrawCache();
    }

    protected override List<ConnectFourMove> GenerateLegalMoves()
    {
        var moves = new List<ConnectFourMove>(Columns);
        if (IsGameWon)
            return moves;

        for (int col = 0; col < Columns; col++)
        {
            if (_topRow[col] >= 0)
                moves.Add(ConnectFourMove.AllMoves[col]);
        }
        return moves;
    }


    #region Board Parsing

    public int GetRowIndexOnDrop(int column)
    {
        int row = _topRow[column] + 1;
        return row;
    }

    private void UpdateWinAndDrawCache()
    {
        _cachedIsGameWon = false;
        _cachedIsGameDraw = false;
        _cachedWinningPlayer = null;
        _cachedWinningCells.Clear();

        if (LastMove.HasValue)
        {
            int row = LastMove.Value.Row;
            int col = LastMove.Value.Col;
            int player = Board[row, col];
            if (HasWinningLine(row, col, player))
            {
                _cachedWinningPlayer = player;
                _cachedIsGameWon = true;
                // Only now get the actual cells for display
                foreach (var (dRow, dCol) in Directions)
                {
                    var cells = GetWinningCells(row, col, dRow, dCol, player);
                    if (cells.Count >= 4)
                    {
                        _cachedWinningCells.Clear();
                        _cachedWinningCells.AddRange(cells.Take(4));
                        break;
                    }
                }
            }
        }

        if (!_cachedIsGameWon)
        {
            // Draw if not win and board full. 
            _cachedIsGameDraw = BoardFull; 
        }
    }

    /// <summary>
    /// Fast check for a winning line from the given cell.
    /// </summary>
    private bool HasWinningLine(int row, int col, int player)
    {
        foreach (var (dRow, dCol) in Directions)
        {
            int count = 1;
            // Forward
            int r = row + dRow, c = col + dCol;
            while ((uint)r < (uint)Rows && (uint)c < (uint)Columns && Board[r, c] == player)
            {
                count++;
                r += dRow;
                c += dCol;
            }
            // Backward
            r = row - dRow; c = col - dCol;
            while ((uint)r < (uint)Rows && (uint)c < (uint)Columns && Board[r, c] == player)
            {
                count++;
                r -= dRow;
                c -= dCol;
            }
            if (count >= 4)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Get the actual winning cells for a given line direction.
    /// </summary>
    private List<(int Row, int Col)> GetWinningCells(int row, int col, int dRow, int dCol, int player)
    {
        Span<(int Row, int Col)> buffer = stackalloc (int Row, int Col)[7];
        int count = 0;
        buffer[count++] = (row, col);

        // Forward
        int r = row + dRow, c = col + dCol;
        while ((uint)r < (uint)Rows && (uint)c < (uint)Columns && Board[r, c] == player)
        {
            buffer[count++] = (r, c);
            r += dRow;
            c += dCol;
        }
        // Backward
        r = row - dRow; c = col - dCol;
        while ((uint)r < (uint)Rows && (uint)c < (uint)Columns && Board[r, c] == player)
        {
            buffer[count++] = (r, c);
            r -= dRow;
            c -= dCol;
        }
        var result = new List<(int Row, int Col)>(count);
        for (int i = 0; i < count; i++)
            result.Add(buffer[i]);
        return result;
    }

    #endregion

    protected override IGameState<ConnectFourMove> CloneInternal()
    {
        var clone = new ConnectFourGameState
        {
            Board = (int[,])Board.Clone(),
            CurrentPlayer = CurrentPlayer,
            MovesMade = MovesMade,
            _topRow = (int[])_topRow.Clone()
        };
        return clone;
    }

    
    public bool Equals(ConnectFourGameState? other)
    {
        if (other == null) return false;
        if (CurrentPlayer != other.CurrentPlayer || MovesMade != other.MovesMade) return false;

        for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Columns; c++)
                if (Board[r, c] != other.Board[r, c])
                    return false;

        return true;
    }

    protected override int GenerateHashCode()
    {
        var hash = new HashCode();
        for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Columns; c++)
            hash.Add(Board[r, c]);
        hash.Add(CurrentPlayer);
        return hash.ToHashCode();
    }

    // Methods to allow controlled setting of Board and CurrentPlayer for testing purposes
    public void SetBoard(int[,] board)
    {
        if (board.GetLength(0) != Rows || board.GetLength(1) != Columns)
            throw new ArgumentException($"Board must be {Rows}x{Columns}.");
        Board = (int[,])board.Clone();

        // Update _topRow based on the provided board  
        for (int col = 0; col < Columns; col++)
        {
            int row = Rows - 1;
            while (row >= 0 && Board[row, col] != 0)
            {
                row--;
            }
            _topRow[col] = row;
        }
        UpdateWinAndDrawCache();
    }

    public void SetCurrentPlayer(int player)
    {
        if (player != 1 && player != 2)
            throw new ArgumentException("CurrentPlayer must be 1 or 2.");
        CurrentPlayer = player;
    }
}
