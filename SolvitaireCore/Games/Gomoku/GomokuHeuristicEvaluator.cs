using System.Xml.Serialization;

namespace SolvitaireCore.Gomoku;

public class GomokuHeuristicEvaluator : StateEvaluator<GomokuGameState, GomokuMove>
{
    // === Heuristic Weights ===
    public int WeightTwoOpen { get; set; } = 10;
    public int WeightThreeOpen { get; set; } = 100;
    public int WeightFourOpen { get; set; } = 10000;
    public int WeightFive { get; set; } = 1_000_000;

    public int WeightTwoGap { get; set; } = 8;
    public int WeightThreeGap { get; set; } = 80;
    public int WeightFourGap { get; set; } = 8000;

    public int WeightTouchingOwn { get; set; } = 2;
    public int WeightTouchingOpponent { get; set; } = -1;

    /// <summary>
    /// Weight for number of free spaces surrounding a player's stone.
    /// </summary>
    public int WeightLiberty { get; set; } = 1;

    public GomokuHeuristicEvaluator()
    {
        MaximumScore = WeightFive;
    }

    public override double EvaluateState(GomokuGameState state, int? maximixingPlayerId = null)
    {
        maximixingPlayerId ??= state.CurrentPlayer;
        if (state.IsGameWon)
        {
            // Positive for current player win, negative for loss
            return state.WinningPlayer == maximixingPlayerId ? MaximumScore : -MaximumScore;
        }
        if (state.IsGameDraw)
            return 0;

        // Heuristic: count open lines of 2, 3, 4 for both players
        double myScore = EvaluateForPlayer(state, maximixingPlayerId.Value);
        double oppScore = EvaluateForPlayer(state, 3 - maximixingPlayerId.Value);

        return myScore - oppScore;
    }

    public override double EvaluateMove(GomokuGameState state, GomokuMove move)
    {
        state.ExecuteMove(move);
        double score = EvaluateState(state);
        state.UndoMove(move);
        return score;
    }

    private double EvaluateForPlayer(GomokuGameState state, int player)
    {
        double score = 0;
        int size = state.BoardSize;
        var board = state.Board;

        var counted = new bool[size, size, 4]; // Prevent double-counting for open lines

        int liberty = 0;
        int touchingOwn = 0, touchingOpponent = 0;
        int two = 0, three = 0, four = 0;
        int twoGap = 0, threeGap = 0, fourGap = 0;

        // Neighbor analysis and unique liberties
        var uniqueLiberties = new HashSet<(int, int)>();

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                if (board[r, c] != player)
                    continue;

                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr == 0 && dc == 0) continue;
                        int nr = r + dr, nc = c + dc;
                        if (nr >= 0 && nr < size && nc >= 0 && nc < size)
                        {
                            int neighbor = board[nr, nc];
                            if (neighbor == 0)
                                uniqueLiberties.Add((nr, nc));
                            else if (neighbor == player)
                                touchingOwn++;
                            else
                                touchingOpponent++;
                        }
                    }
                }
            }
        }
        liberty = uniqueLiberties.Count;

        // Open lines but only if this cell is a player's stone
        for (int dir = 0; dir < GridDirections.Length; dir++)
        {
            var (dr, dc) = GridDirections[dir];
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    if (board[r, c] == player && !counted[r, c, dir])
                    {
                        int count = 1;
                        int openEnds = 0;

                        // Forward
                        int i = 1;
                        while (true)
                        {
                            int nr = r + dr * i, nc = c + dc * i;
                            if (nr < 0 || nr >= size || nc < 0 || nc >= size)
                                break;
                            if (board[nr, nc] == player)
                            {
                                counted[nr, nc, dir] = true;
                                count++;
                                i++;
                            }
                            else
                            {
                                if (board[nr, nc] == 0)
                                    openEnds++;
                                break;
                            }
                        }
                        // Backward
                        i = 1;
                        while (true)
                        {
                            int nr = r - dr * i, nc = c - dc * i;
                            if (nr < 0 || nr >= size || nc < 0 || nc >= size)
                                break;
                            if (board[nr, nc] == player)
                            {
                                counted[nr, nc, dir] = true;
                                count++;
                                i++;
                            }
                            else
                            {
                                if (board[nr, nc] == 0)
                                    openEnds++;
                                break;
                            }
                        }

                        if (count == 4 && openEnds > 0)
                            four++;
                        else if (count == 3 && openEnds > 0)
                            three++;
                        else if (count == 2 && openEnds > 0)
                            two++;
                    }
                }
            }
        }

        // --- Lines with gaps: use sliding window, only count canonical windows ---
        for (int dir = 0; dir < GridDirections.Length; dir++)
        {
            var (dr, dc) = GridDirections[dir];

            // 2-gap: window size 3 (X . X)
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    int[] windowR = new int[3];
                    int[] windowC = new int[3];
                    bool inBounds = true;
                    for (int i = 0; i < 3; i++)
                    {
                        int nr = r + dr * i, nc = c + dc * i;
                        windowR[i] = nr;
                        windowC[i] = nc;
                        if (nr < 0 || nr >= size || nc < 0 || nc >= size)
                        {
                            inBounds = false;
                            break;
                        }
                    }
                    if (!inBounds) continue;

                    // Only count X . X (gap in the middle)
                    if (board[windowR[0], windowC[0]] == player &&
                        board[windowR[1], windowC[1]] == 0 &&
                        board[windowR[2], windowC[2]] == player)
                    {
                        // Only count if not part of a longer line
                        int beforeR = r - dr, beforeC = c - dc;
                        int afterR = r + dr * 3, afterC = c + dc * 3;
                        bool beforeOnBoard = beforeR >= 0 && beforeR < size && beforeC >= 0 && beforeC < size;
                        bool afterOnBoard = afterR >= 0 && afterR < size && afterC >= 0 && afterC < size;
                        bool beforeIsPlayer = beforeOnBoard && board[beforeR, beforeC] == player;
                        bool afterIsPlayer = afterOnBoard && board[afterR, afterC] == player;
                        if (!beforeIsPlayer && !afterIsPlayer)
                            twoGap++;
                    }
                }
            }

            // 3-gap: window size 4 (X X . X, X . X X, X X X .)
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    int[] windowR = new int[4];
                    int[] windowC = new int[4];
                    bool inBounds = true;
                    for (int i = 0; i < 4; i++)
                    {
                        int nr = r + dr * i, nc = c + dc * i;
                        windowR[i] = nr;
                        windowC[i] = nc;
                        if (nr < 0 || nr >= size || nc < 0 || nc >= size)
                        {
                            inBounds = false;
                            break;
                        }
                    }
                    if (!inBounds) continue;

                    // Only count if exactly 3 stones, 1 empty, no opponent, and not all consecutive
                    int playerCount = 0, emptyCount = 0, oppCount = 0, emptyIdx = -1;
                    for (int i = 0; i < 4; i++)
                    {
                        int cell = board[windowR[i], windowC[i]];
                        if (cell == player) playerCount++;
                        else if (cell == 0) { emptyCount++; emptyIdx = i; }
                        else oppCount++;
                    }
                    if (playerCount == 3 && emptyCount == 1 && oppCount == 0)
                    {
                        // Only count if the empty is at the leftmost possible position for this pattern
                        // (i.e., only count if emptyIdx == 1, so X . X X, or emptyIdx == 2, so X X . X)
                        // This avoids double-counting for overlapping windows
                        if (emptyIdx == 1 || emptyIdx == 2)
                        {
                            int beforeR = r - dr, beforeC = c - dc;
                            int afterR = r + dr * 4, afterC = c + dc * 4;
                            bool beforeOnBoard = beforeR >= 0 && beforeR < size && beforeC >= 0 && beforeC < size;
                            bool afterOnBoard = afterR >= 0 && afterR < size && afterC >= 0 && afterC < size;
                            bool beforeIsPlayer = beforeOnBoard && board[beforeR, beforeC] == player;
                            bool afterIsPlayer = afterOnBoard && board[afterR, afterC] == player;
                            if (!beforeIsPlayer && !afterIsPlayer)
                                threeGap++;
                        }
                    }
                }
            }

            // 4-gap: window size 5 (X X X . X, X X . X X, X . X X X)
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    int[] windowR = new int[5];
                    int[] windowC = new int[5];
                    bool inBounds = true;
                    for (int i = 0; i < 5; i++)
                    {
                        int nr = r + dr * i, nc = c + dc * i;
                        windowR[i] = nr;
                        windowC[i] = nc;
                        if (nr < 0 || nr >= size || nc < 0 || nc >= size)
                        {
                            inBounds = false;
                            break;
                        }
                    }
                    if (!inBounds) continue;

                    int playerCount = 0, emptyCount = 0, oppCount = 0, emptyIdx = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        int cell = board[windowR[i], windowC[i]];
                        if (cell == player) playerCount++;
                        else if (cell == 0) { emptyCount++; emptyIdx = i; }
                        else oppCount++;
                    }
                    if (playerCount == 4 && emptyCount == 1 && oppCount == 0)
                    {
                        // Only count if the empty is at the leftmost possible position for this pattern
                        // (i.e., only count if emptyIdx == 1, 2, or 3)
                        if (emptyIdx == 1 || emptyIdx == 2 || emptyIdx == 3)
                        {
                            int beforeR = r - dr, beforeC = c - dc;
                            int afterR = r + dr * 5, afterC = c + dc * 5;
                            bool beforeOnBoard = beforeR >= 0 && beforeR < size && beforeC >= 0 && beforeC < size;
                            bool afterOnBoard = afterR >= 0 && afterR < size && afterC >= 0 && afterC < size;
                            bool beforeIsPlayer = beforeOnBoard && board[beforeR, beforeC] == player;
                            bool afterIsPlayer = afterOnBoard && board[afterR, afterC] == player;
                            if (!beforeIsPlayer && !afterIsPlayer)
                                fourGap++;
                        }
                    }
                }
            }
        }


        score += WeightTwoOpen * two;
        score += WeightThreeOpen * three;
        score += WeightFourOpen * four;
        score += WeightTwoGap * twoGap;
        score += WeightThreeGap * threeGap;
        score += WeightFourGap * fourGap;
        score += WeightTouchingOwn * touchingOwn;
        score += WeightTouchingOpponent * touchingOpponent;
        score += WeightLiberty * liberty;

        return score;
    }
}


