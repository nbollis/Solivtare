namespace SolvitaireCore.TicTacToe;

/// <summary>
/// Heuristic evaluator for TicTacToe game states.
/// Scores are positive for the maximizing player, negative for the opponent.
/// </summary>
public class TicTacToeHeuristicEvaluator : StateEvaluator<TicTacToeGameState, TicTacToeMove>
{
    // Heuristic weights (tune as needed)
    private const int WinScore = 100;
    private const int BlockWinScore = 90;

    private const int CenterWeight = 4;
    private const int CornerWeight = 3;
    private const int EdgeWeight = 1;
    private const int TwoInRowWeight = 10;
    private const int BlockTwoInRowWeight = 8;
    private const int TwoWithGapWeight = 6;
    private const int BlockTwoWithGapWeight = 5;

    public TicTacToeHeuristicEvaluator()
    {
        MaximumScore = WinScore;
    }
    public override IEnumerable<(TicTacToeMove Move, double MoveScore)> OrderMoves(List<TicTacToeMove> moves, TicTacToeGameState state, bool bestFirst)
    {
        // Fast, shallow scoring for move ordering
        var scoredMoves = moves.Select(move =>
        {
            double score = 0;
            // Prefer center
            if (move.Row == 1 && move.Col == 1)
                score += CenterWeight;
            // Prefer corners
            else if ((move.Row == 0 || move.Row == 2) && (move.Col == 0 || move.Col == 2))
                score += CornerWeight;
            // Prefer edges
            else
                score += EdgeWeight;

            // Check for immediate win/block
            state.ExecuteMove(move);
            if (state.IsPlayerWin(state.CurrentPlayer == 1 ? 2 : 1)) // Opponent would win if we don't block
                score += BlockWinScore;
            else if (state.IsPlayerWin(state.CurrentPlayer))
                score += WinScore;
            state.UndoMove(move);

            return (Move: move, MoveScore: score);
        });

        return bestFirst
            ? scoredMoves.OrderByDescending(m => m.MoveScore)
            : scoredMoves.OrderBy(m => m.MoveScore);
    }

    public override double EvaluateMove(TicTacToeGameState state, TicTacToeMove move)
    {
        int currentPlayer = state.CurrentPlayer;
        state.ExecuteMove(move);

        double score;
        if (state.IsPlayerWin(currentPlayer))
            score = MaximumScore; // Immediate win
        else if (state.IsPlayerLoss(currentPlayer))
            score = -MaximumScore; // Immediate loss (should block)
        else if (state.IsGameDraw)
            score = 0;
        else
            score = EvaluateState(state, currentPlayer);

        state.UndoMove(move);
        return score;
    }

    public override double EvaluateState(TicTacToeGameState state, int? maximizingPlayerId = null)
    {
        maximizingPlayerId ??= state.CurrentPlayer;
        int player = maximizingPlayerId.Value;
        int opponent = 3 - player;

        if (state.IsPlayerWin(player))
            return MaximumScore;
        if (state.IsPlayerLoss(player))
            return -MaximumScore;
        if (state.IsGameDraw)
            return 0;

        int[,] board = state.Board;
        int score = 0;

        // Center control
        if (board[1, 1] == player)
            score += CenterWeight;
        else if (board[1, 1] == opponent)
            score -= CenterWeight;

        // Corner control
        foreach (var (r, c) in new[] { (0, 0), (0, 2), (2, 0), (2, 2) })
        {
            if (board[r, c] == player)
                score += CornerWeight;
            else if (board[r, c] == opponent)
                score -= CornerWeight;
        }

        // Edge control
        foreach (var (r, c) in new[] { (0, 1), (1, 0), (1, 2), (2, 1) })
        {
            if (board[r, c] == player)
                score += EdgeWeight;
            else if (board[r, c] == opponent)
                score -= EdgeWeight;
        }

        // Two-in-a-row and block two-in-a-row
        score += TwoInRowWeight * CountTwoInRow(board, player);
        score -= BlockTwoInRowWeight * CountTwoInRow(board, opponent);

        // Two-with-gap and block two-with-gap
        score += TwoWithGapWeight * CountTwoWithGap(board, player);
        score -= BlockTwoWithGapWeight * CountTwoWithGap(board, opponent);

        return Math.Clamp(score, -MaximumScore, MaximumScore);

        return Math.Clamp(score, -MaximumScore, MaximumScore);
    }

    /// <summary>
    /// Counts the number of lines (row, col, diag) where the player has two and the third is empty.
    /// </summary>
    private static int CountTwoInRow(int[,] board, int player)
    {
        int count = 0;
        // Rows
        for (int r = 0; r < 3; r++)
        {
            int p = 0, empty = 0;
            for (int c = 0; c < 3; c++)
            {
                if (board[r, c] == player) p++;
                else if (board[r, c] == 0) empty++;
            }
            if (p == 2 && empty == 1) count++;
        }
        // Columns
        for (int c = 0; c < 3; c++)
        {
            int p = 0, empty = 0;
            for (int r = 0; r < 3; r++)
            {
                if (board[r, c] == player) p++;
                else if (board[r, c] == 0) empty++;
            }
            if (p == 2 && empty == 1) count++;
        }
        // Main diagonal
        {
            int p = 0, empty = 0;
            for (int i = 0; i < 3; i++)
            {
                if (board[i, i] == player) p++;
                else if (board[i, i] == 0) empty++;
            }
            if (p == 2 && empty == 1) count++;
        }
        // Anti-diagonal
        {
            int p = 0, empty = 0;
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 2 - i] == player) p++;
                else if (board[i, 2 - i] == 0) empty++;
            }
            if (p == 2 && empty == 1) count++;
        }
        return count;
    }

    /// <summary>
    /// Counts the number of lines (row, col, diag) where the player has two marks with a gap between them and the third cell is empty.
    /// For TicTacToe, this means patterns like [X, empty, X] or [empty, X, X] or [X, X, empty] but not consecutive.
    /// </summary>
    private static int CountTwoWithGap(int[,] board, int player)
    {
        int count = 0;
        // Rows
        for (int r = 0; r < 3; r++)
        {
            int[] line = { board[r, 0], board[r, 1], board[r, 2] };
            if (IsTwoWithGap(line, player)) count++;
        }
        // Columns
        for (int c = 0; c < 3; c++)
        {
            int[] line = { board[0, c], board[1, c], board[2, c] };
            if (IsTwoWithGap(line, player)) count++;
        }
        // Main diagonal
        {
            int[] line = { board[0, 0], board[1, 1], board[2, 2] };
            if (IsTwoWithGap(line, player)) count++;
        }
        // Anti-diagonal
        {
            int[] line = { board[0, 2], board[1, 1], board[2, 0] };
            if (IsTwoWithGap(line, player)) count++;
        }
        return count;
    }

    /// <summary>
    /// Returns true if the line has exactly two of the player's marks and one empty, and the two marks are not adjacent.
    /// </summary>
    private static bool IsTwoWithGap(int[] line, int player)
    {
        // Must have two of player's marks and one empty
        int playerCount = line.Count(x => x == player);
        int emptyCount = line.Count(x => x == 0);
        if (playerCount != 2 || emptyCount != 1)
            return false;

        // Check for gap: the empty is between the two marks
        // [X, 0, X]
        if (line[0] == player && line[1] == 0 && line[2] == player)
            return true;

        // [0, X, X] or [X, X, 0] are not "with gap" (they are consecutive)
        return false;
    }
}
