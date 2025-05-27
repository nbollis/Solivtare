namespace SolvitaireCore.ConnectFour;

public class ConnectFourHeuristicEvaluator : StateEvaluator<ConnectFourGameState, ConnectFourMove>
{
    private static readonly (int dr, int dc)[] Directions = new[] { (0, 1), (1, 0), (1, 1), (1, -1) };

    // Heuristic weights (tune as needed)
    private const int CenterWeight = 3;
    private const int TwoInARowWeight = 10;
    private const int TwoWithGapWeight = 6;
    private const int ThreeInARowWeight = 50;
    private const int ThreeWithGapWeight = 30;
    private const int IsolatedWeight = -4;
    private const int SurroundedWeight = -8;
    private const int TouchingOwnWeight = 2;
    private const int TouchingOpponentWeight = -1;

    public override IEnumerable<(ConnectFourMove Move, double MoveScore)> OrderMoves(List<ConnectFourMove> moves, ConnectFourGameState state, bool bestFirst)
    {
        // Fast, shallow scoring for ordering
        var scoredMoves = moves.Select(move =>
        {
            // Prefer center columns
            int centerBonus = 3 - Math.Abs(move.Column - 3);

            // Check for immediate win/block
            state.ExecuteMove(move);
            double score = 0;
            if (state.IsPlayerWin(state.CurrentPlayer == 1 ? 2 : 1)) // Opponent would win if we don't block
                score = MaximumScore * 0.9;
            else if (state.IsPlayerWin(state.CurrentPlayer))
                score = MaximumScore;
            state.UndoMove(move);

            score += centerBonus * CenterWeight;
            return (Move: move, MoveScore: score);
        });

        return bestFirst
            ? scoredMoves.OrderByDescending(m => m.MoveScore)
            : scoredMoves.OrderBy(m => m.MoveScore);
    }

    public override double EvaluateMove(ConnectFourGameState state, ConnectFourMove move)
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

    public override double EvaluateState(ConnectFourGameState state, int? maximixingPlayerId = null)
    {
        maximixingPlayerId ??= state.CurrentPlayer;

        if (state.IsPlayerWin(maximixingPlayerId.Value))
            return MaximumScore;
        if (state.IsPlayerLoss(maximixingPlayerId.Value))
            return -MaximumScore;

        int player = maximixingPlayerId.Value;
        int opponent = 3 - player;

        int[,] board = state.Board;
        int rows = ConnectFourGameState.Rows;
        int cols = ConnectFourGameState.Columns;

        int playerIsolated = 0, opponentIsolated = 0;
        int playerSurrounded = 0, opponentSurrounded = 0;
        int playerTwo = 0, playerTwoGap = 0, playerThree = 0, playerThreeGap = 0;
        int opponentTwo = 0, opponentTwoGap = 0, opponentThree = 0, opponentThreeGap = 0;
        int playerTouching = 0, playerTouchingOpponent = 0;
        int opponentTouching = 0, opponentTouchingPlayer = 0;
        int playerCenter = 0, opponentCenter = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int cell = board[row, col];
                if (cell == 0) continue;

                // Center column bonus
                if (col == 3)
                {
                    if (cell == player) playerCenter++;
                    else opponentCenter++;
                }

                // Neighbor analysis
                int sameNeighbors = 0, touchingOwn = 0, touchingOpponent = 0;
                int totalNeighbors = 0, nonEmptyNeighbors = 0;

                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr == 0 && dc == 0) continue;
                        int nr = row + dr, nc = col + dc;
                        if ((uint)nr < (uint)rows && (uint)nc < (uint)cols)
                        {
                            int neighbor = board[nr, nc];
                            totalNeighbors++;
                            if (neighbor != 0) nonEmptyNeighbors++;
                            if (neighbor == cell) { sameNeighbors++; touchingOwn++; }
                            else if (neighbor != 0) touchingOpponent++;
                        }
                        else
                        {
                            totalNeighbors++;
                            nonEmptyNeighbors++;
                        }
                    }
                }

                if (cell == player)
                {
                    playerTouching += touchingOwn;
                    playerTouchingOpponent += touchingOpponent;
                }
                else
                {
                    opponentTouching += touchingOwn;
                    opponentTouchingPlayer += touchingOpponent;
                }

                if (sameNeighbors == 0)
                {
                    if (cell == player) playerIsolated++;
                    else opponentIsolated++;
                }

                if (nonEmptyNeighbors == totalNeighbors)
                {
                    if (cell == player) playerSurrounded++;
                    else opponentSurrounded++;
                }

                // N-in-a-row and N-with-gap
                foreach (var (dr, dc) in Directions)
                {
                    // 2-in-a-row
                    int r1 = row + dr, c1 = col + dc;
                    if ((uint)r1 < (uint)rows && (uint)c1 < (uint)cols)
                    {
                        if (cell == player && board[r1, c1] == player)
                            playerTwo++;
                        if (cell == opponent && board[r1, c1] == opponent)
                            opponentTwo++;
                    }

                    // 3-in-a-row
                    int r2 = row + 2 * dr, c2 = col + 2 * dc;
                    if ((uint)r1 < (uint)rows && (uint)c1 < (uint)cols &&
                        (uint)r2 < (uint)rows && (uint)c2 < (uint)cols)
                    {
                        if (cell == player && board[r1, c1] == player && board[r2, c2] == player)
                            playerThree++;
                        if (cell == opponent && board[r1, c1] == opponent && board[r2, c2] == opponent)
                            opponentThree++;
                    }

                    // 2-with-gap and 3-with-gap
#pragma warning disable CA2014
                    // ReSharper disable once StackAllocInsideLoop
                    Span<int> window = stackalloc int[4];
#pragma warning restore CA2014
                    bool fullWindow = true;
                    int wr = row, wc = col;
                    for (int i = 0; i < 4; i++, wr += dr, wc += dc)
                    {
                        if ((uint)wr >= (uint)rows || (uint)wc >= (uint)cols)
                        {
                            fullWindow = false;
                            break;
                        }
                        window[i] = board[wr, wc];
                    }
                    if (fullWindow)
                    {
                        int playerCount = 0, opponentCount = 0, emptyCount = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            if (window[i] == player) playerCount++;
                            else if (window[i] == opponent) opponentCount++;
                            else if (window[i] == 0) emptyCount++;
                        }
                        if (playerCount == 2 && opponentCount == 0 && emptyCount == 2 && HasGapWindow(window, player, 2))
                            playerTwoGap++;
                        if (playerCount == 3 && opponentCount == 0 && emptyCount == 1 && HasGapWindow(window, player, 3))
                            playerThreeGap++;
                        if (opponentCount == 2 && playerCount == 0 && emptyCount == 2 && HasGapWindow(window, opponent, 2))
                            opponentTwoGap++;
                        if (opponentCount == 3 && playerCount == 0 && emptyCount == 1 && HasGapWindow(window, opponent, 3))
                            opponentThreeGap++;
                    }
                }
            }
        }

        // Weighted sum of all features
        double score = 0;
        score += CenterWeight * playerCenter;
        score += TwoInARowWeight * playerTwo;
        score += TwoWithGapWeight * playerTwoGap;
        score += ThreeInARowWeight * playerThree;
        score += ThreeWithGapWeight * playerThreeGap;
        score += IsolatedWeight * playerIsolated;
        score += SurroundedWeight * playerSurrounded;
        score += TouchingOwnWeight * playerTouching;
        score += TouchingOpponentWeight * playerTouchingOpponent;

        score -= CenterWeight * opponentCenter;
        score -= TwoInARowWeight * opponentTwo;
        score -= TwoWithGapWeight * opponentTwoGap;
        score -= ThreeInARowWeight * opponentThree;
        score -= ThreeWithGapWeight * opponentThreeGap;
        score -= IsolatedWeight * opponentIsolated;
        score -= SurroundedWeight * opponentSurrounded;
        score -= TouchingOwnWeight * opponentTouching;
        score -= TouchingOpponentWeight * opponentTouchingPlayer;

        // Clamp to maximum score range
        return Math.Clamp(score, -MaximumScore, MaximumScore);
    }

    private static bool HasGapWindow(ReadOnlySpan<int> window, int player, int n)
    {
        int count = 0, empty = 0;
        for (int i = 0; i < 4; i++)
        {
            if (window[i] == player) count++;
            else if (window[i] == 0) empty++;
        }
        if (count != n || empty != 4 - n) return false;

        // Check for a gap: not all pieces consecutive
        int maxConsecutive = 0, consecutive = 0;
        for (int i = 0; i < 4; i++)
        {
            if (window[i] == player)
            {
                consecutive++;
                if (consecutive > maxConsecutive) maxConsecutive = consecutive;
            }
            else
            {
                consecutive = 0;
            }
        }
        return maxConsecutive < n;
    }
}