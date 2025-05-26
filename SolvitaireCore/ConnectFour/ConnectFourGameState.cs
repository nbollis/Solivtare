namespace SolvitaireCore.ConnectFour;

public class ConnectFourGameState : ITwoPlayerGameState<ConnectFourMove>, IEquatable<ConnectFourGameState>
{
    public const int Rows = 6;
    public const int Columns = 7; 
    private static readonly (int dRow, int dCol)[] Directions = [(1, 0), (0, 1), (1, 1), (1, -1)];

    // Cacheing to reduce redundant calculations. 
    private int _cachedHash = 0;
    private bool _hashDirty = true;
    private bool _cachedIsGameWon = false;
    private bool _cachedIsGameDraw = false;
    private (int Row, int Col)? _lastMove = null;
    private int? _cachedWinningPlayer = null;
    private readonly List<(int Row, int Col)> _cachedWinningCells = new();
    private int[] _topRow = Enumerable.Repeat(Rows - 1, Columns).ToArray();

    public bool IsGameWon => _cachedIsGameWon;
    public bool IsGameDraw => _cachedIsGameDraw;
    public bool IsGameLost => false; // Implement if you want to track explicit losses
    public int? WinningPlayer => _cachedWinningPlayer;
    public IReadOnlyList<(int Row, int Col)> WinningCells => _cachedWinningCells;


    // 0 = empty, 1 = player1, 2 = player2
    public int[,] Board { get; private set; } = new int[Rows, Columns];
    public int CurrentPlayer { get; private set; } = 1;
    public int MovesMade { get; private set; } = 0;
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

    public void ExecuteMove(ConnectFourMove move)
    {
        int row = _topRow[move.Column];
        if (row < 0)
            throw new InvalidOperationException("Invalid move: column full");
        Board[row, move.Column] = CurrentPlayer;
        MovesMade++;
        _moveHistory.Add(move);
        _lastMove = (row, move.Column);
        CurrentPlayer = 3 - CurrentPlayer;
        _topRow[move.Column]--;
        UpdateWinAndDrawCache();
    }

    public void UndoMove(ConnectFourMove move)
    {
        int row = _topRow[move.Column] + 1;
        if (row >= Rows || Board[row, move.Column] == 0)
            throw new InvalidOperationException("Invalid undo: column empty");
        Board[row, move.Column] = 0;
        MovesMade--;
        CurrentPlayer = 3 - CurrentPlayer;
        if (_moveHistory.Count > 0)
            _moveHistory.RemoveAt(_moveHistory.Count - 1);
        _topRow[move.Column]++;
        _lastMove = null;
        UpdateWinAndDrawCache();
    }

    public void Reset()
    {
        Board = new int[Rows, Columns];
        CurrentPlayer = 1;
        MovesMade = 0;
        _moveHistory.Clear();
        _topRow = Enumerable.Repeat(Rows - 1, Columns).ToArray();
        _lastMove = null;
        UpdateWinAndDrawCache();
    }

    public List<ConnectFourMove> GetLegalMoves()
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

    #region Move History Tracking

    private readonly List<ConnectFourMove> _moveHistory = new();
    public IReadOnlyList<ConnectFourMove> GetMoveHistory() => _moveHistory.AsReadOnly();
    public string GetMoveHistoryString()
    {
        return string.Join(",", _moveHistory.Select(m => m.Column));
    }

    #endregion

    #region Board Parsing
   
    private void UpdateWinAndDrawCache()
    {
        _cachedIsGameWon = false;
        _cachedIsGameDraw = false;
        _cachedWinningPlayer = null;
        _cachedWinningCells.Clear();

        if (_lastMove.HasValue)
        {
            int row = _lastMove.Value.Row;
            int col = _lastMove.Value.Col;
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

    public IGameState<ConnectFourMove> Clone()
    {
        var clone = new ConnectFourGameState
        {
            Board = (int[,])Board.Clone(),
            CurrentPlayer = CurrentPlayer,
            MovesMade = MovesMade,
            _topRow = (int[])_topRow.Clone()
        };
        clone._moveHistory.AddRange(_moveHistory);
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

    public override int GetHashCode()
    {
        if (!_hashDirty) 
            return _cachedHash;

        int hash = 17;
        foreach (var cell in Board)
            hash = hash * 31 + cell;
        hash = hash * 31 + CurrentPlayer;

        _cachedHash = hash;
        _hashDirty = false;
        return hash;
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
