namespace SolvitaireCore.Gomoku;

public class GomokuGameState : BaseGameState<GomokuMove>,
    ITwoPlayerGameState<GomokuMove>, IEquatable<GomokuGameState>
{
    public const int DefaultSize = 15;
    public int BoardSize { get; }
    public int[,] Board { get; private set; }
    public int CurrentPlayer { get; private set; } = 1; // 1 = Black, 2 = White
    public (int Row, int Col)? LastMove { get; private set; }
    private int? _cachedWinningPlayer = null;
    private readonly List<(int Row, int Col)> _cachedWinningCells = new();
    private bool _cachedIsGameWon = false;
    private bool _cachedIsGameDraw = false;

    public override bool IsGameWon => _cachedIsGameWon;
    public bool IsGameDraw => _cachedIsGameDraw;
    public override bool IsGameLost => false;
    public int? WinningPlayer => _cachedWinningPlayer;
    public IReadOnlyList<(int Row, int Col)> WinningCells => _cachedWinningCells;

    public GomokuGameState(int boardSize = DefaultSize)
    {
        BoardSize = boardSize;
        Board = new int[BoardSize, BoardSize];
        ResetInternal();
    }

    protected override void ResetInternal()
    {
        Board = new int[BoardSize, BoardSize];
        CurrentPlayer = 1;
        MovesMade = 0;
        LastMove = null;
        _cachedWinningPlayer = null;
        _cachedWinningCells.Clear();
        _cachedIsGameWon = false;
        _cachedIsGameDraw = false;
    }

    protected override List<GomokuMove> GenerateLegalMoves()
    {
        var moves = new List<GomokuMove>(BoardSize * BoardSize);
        if (IsGameWon)
            return moves;

        for (int r = 0; r < BoardSize; r++)
        for (int c = 0; c < BoardSize; c++)
            if (Board[r, c] == 0)
                moves.Add(new GomokuMove(r, c));
        return moves;
    }

    protected override void ExecuteMoveInternal(GomokuMove move)
    {
        if (Board[move.Row, move.Col] != 0)
            throw new InvalidOperationException("Cell already occupied.");

        if (IsGameWon)
            return;

        Board[move.Row, move.Col] = CurrentPlayer;
        LastMove = (move.Row, move.Col);
        CurrentPlayer = 3 - CurrentPlayer;
        UpdateWinAndDrawCache(move.Row, move.Col);
    }

    protected override void UndoMoveInternal(GomokuMove move)
    {
        if (Board[move.Row, move.Col] == 0)
            throw new InvalidOperationException("Cell already empty.");

        Board[move.Row, move.Col] = 0;
        CurrentPlayer = 3 - CurrentPlayer;
        LastMove = null;
        UpdateWinAndDrawCache();
    }

    protected override GomokuGameState CloneInternal()
    {
        var clone = new GomokuGameState(BoardSize)
        {
            Board = (int[,])Board.Clone(),
            CurrentPlayer = CurrentPlayer,
            MovesMade = MovesMade,
            LastMove = LastMove
        };
        clone._cachedWinningPlayer = _cachedWinningPlayer;
        clone._cachedWinningCells.AddRange(_cachedWinningCells);
        clone._cachedIsGameWon = _cachedIsGameWon;
        clone._cachedIsGameDraw = _cachedIsGameDraw;
        return clone;
    }

    private static readonly (int dRow, int dCol)[] Directions = [(1, 0), (0, 1), (1, 1), (1, -1)];

    private void UpdateWinAndDrawCache(int? lastRow = null, int? lastCol = null)
    {
        _cachedIsGameWon = false;
        _cachedIsGameDraw = false;
        _cachedWinningPlayer = null;
        _cachedWinningCells.Clear();

        if (lastRow.HasValue && lastCol.HasValue && Board[lastRow.Value, lastCol.Value] != 0)
        {
            int player = Board[lastRow.Value, lastCol.Value];
            foreach (var (dRow, dCol) in Directions)
            {
                var cells = GetWinningCells(lastRow.Value, lastCol.Value, dRow, dCol, player);
                if (cells.Count >= 5)
                {
                    _cachedWinningPlayer = player;
                    _cachedIsGameWon = true;
                    _cachedWinningCells.AddRange(cells.Take(5));
                    return;
                }
            }
        }

        // Draw if not win and board full
        _cachedIsGameDraw = !Board.Cast<int>().Any(cell => cell == 0) && !_cachedIsGameWon;
    }

    private List<(int Row, int Col)> GetWinningCells(int row, int col, int dRow, int dCol, int player)
    {
        var buffer = new List<(int Row, int Col)>(9) { (row, col) };

        // Forward
        int r = row + dRow, c = col + dCol;
        while (IsOnBoard(r, c) && Board[r, c] == player)
        {
            buffer.Add((r, c));
            r += dRow;
            c += dCol;
        }
        // Backward
        r = row - dRow; c = col - dCol;
        while (IsOnBoard(r, c) && Board[r, c] == player)
        {
            buffer.Add((r, c));
            r -= dRow;
            c -= dCol;
        }
        return buffer;
    }

    private bool IsOnBoard(int row, int col) =>
        row >= 0 && row < BoardSize && col >= 0 && col < BoardSize;

    public bool IsPlayerWin(int player) => IsGameWon && WinningPlayer == player;
    public bool IsPlayerLoss(int player) => IsGameWon && WinningPlayer != player;

    public bool Equals(GomokuGameState? other)
    {
        if (other == null) return false;
        if (CurrentPlayer != other.CurrentPlayer || MovesMade != other.MovesMade) return false;
        if (BoardSize != other.BoardSize) return false;
        for (int r = 0; r < BoardSize; r++)
        for (int c = 0; c < BoardSize; c++)
            if (Board[r, c] != other.Board[r, c])
                return false;
        return true;
    }

    protected override int GenerateHashCode()
    {
        var hash = new HashCode();
        for (int r = 0; r < BoardSize; r++)
        for (int c = 0; c < BoardSize; c++)
            hash.Add(Board[r, c]);
        hash.Add(CurrentPlayer);
        return hash.ToHashCode();
    }
    public void SetCurrentPlayer(int player)
    {
        if (player != 1 && player != 2)
            throw new ArgumentException("CurrentPlayer must be 1 or 2.");
        CurrentPlayer = player;
    }
}