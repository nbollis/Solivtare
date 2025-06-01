using System.Drawing;
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
        int size = state.BoardSize;
        var board = state.Board;
        int center = size / 2;

        int r = move.Row, c = move.Col;
        int adjacent = 0;
        for (int dr = -1; dr <= 1; dr++)
        {
            for (int dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                int nr = r + dr, nc = c + dc;
                if (nr >= 0 && nr < size && nc >= 0 && nc < size)
                {
                    if (board[nr, nc] != 0)
                        adjacent++;
                }
            }
        }
        // Optionally, prefer moves closer to the center early in the game
        double distToCenter = Math.Sqrt((r - center) * (r - center) + (c - center) * (c - center));
        return adjacent * 100 - distToCenter;
    }

    private HashSetPool<(int, int)> _hasSetPool = new HashSetPool<(int, int)>(32);
    private double EvaluateForPlayer(GomokuGameState state, int player)
    {
        double score = 0;
        int size = state.BoardSize;
        var board = state.Board;

        var counted = new bool[size, size, 4]; // Prevent double-counting for open lines

        int liberties = 0;
        int touchingOwn = 0, touchingOpponent = 0;
        int two = 0, three = 0, four = 0;
        int twoGap = 0, threeGap = 0, fourGap = 0;

        // --- Neighbor analysis and unique liberties in one pass ---
        var uniqueLiberties = _hasSetPool.Get();
        try
        {
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
                            if ((uint)nr < (uint)size && (uint)nc < (uint)size)
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
            
        }
        finally
        {
            liberties = uniqueLiberties.Count;
            _hasSetPool.Return(uniqueLiberties);
        }

        // --- Open lines (2, 3, 4 in a row with open ends) ---
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
                            if ((uint)nr >= (uint)size || (uint)nc >= (uint)size)
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
                            if ((uint)nr >= (uint)size || (uint)nc >= (uint)size)
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

        // --- Lines with gaps: sliding window, only canonical windows ---
        for (int dir = 0; dir < GridDirections.Length; dir++)
        {
            var (dr, dc) = GridDirections[dir];

            // 2-gap: window size 3 (X . X)
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    int r0 = r, c0 = c, r1 = r + dr, c1 = c + dc, r2 = r + dr * 2, c2 = c + dc * 2;
                    if ((uint)r2 >= (uint)size || (uint)c2 >= (uint)size)
                        continue;
                    if (board[r0, c0] == player && board[r1, c1] == 0 && board[r2, c2] == player)
                    {
                        int beforeR = r - dr, beforeC = c - dc;
                        int afterR = r + dr * 3, afterC = c + dc * 3;
                        bool beforeOnBoard = (uint)beforeR < (uint)size && (uint)beforeC < (uint)size;
                        bool afterOnBoard = (uint)afterR < (uint)size && (uint)afterC < (uint)size;
                        bool beforeIsPlayer = beforeOnBoard && board[beforeR, beforeC] == player;
                        bool afterIsPlayer = afterOnBoard && board[afterR, afterC] == player;
                        if (!beforeIsPlayer && !afterIsPlayer)
                            twoGap++;
                    }
                }
            }

            // 3-gap: window size 4 (X X . X, X . X X)
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    int[] rr = { r, r + dr, r + dr * 2, r + dr * 3 };
                    int[] cc = { c, c + dc, c + dc * 2, c + dc * 3 };
                    if ((uint)rr[3] >= (uint)size || (uint)cc[3] >= (uint)size)
                        continue;
                    int playerCount = 0, emptyCount = 0, oppCount = 0, emptyIdx = -1;
                    for (int i = 0; i < 4; i++)
                    {
                        int cell = board[rr[i], cc[i]];
                        if (cell == player) playerCount++;
                        else if (cell == 0) { emptyCount++; emptyIdx = i; }
                        else oppCount++;
                    }
                    if (playerCount == 3 && emptyCount == 1 && oppCount == 0 && (emptyIdx == 1 || emptyIdx == 2))
                    {
                        int beforeR = r - dr, beforeC = c - dc;
                        int afterR = r + dr * 4, afterC = c + dc * 4;
                        bool beforeOnBoard = (uint)beforeR < (uint)size && (uint)beforeC < (uint)size;
                        bool afterOnBoard = (uint)afterR < (uint)size && (uint)afterC < (uint)size;
                        bool beforeIsPlayer = beforeOnBoard && board[beforeR, beforeC] == player;
                        bool afterIsPlayer = afterOnBoard && board[afterR, afterC] == player;
                        if (!beforeIsPlayer && !afterIsPlayer)
                            threeGap++;
                    }
                }
            }

            // 4-gap: window size 5 (X X X . X, X X . X X, X . X X X)
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    int[] rr = { r, r + dr, r + dr * 2, r + dr * 3, r + dr * 4 };
                    int[] cc = { c, c + dc, c + dc * 2, c + dc * 3, c + dc * 4 };
                    if ((uint)rr[4] >= (uint)size || (uint)cc[4] >= (uint)size)
                        continue;
                    int playerCount = 0, emptyCount = 0, oppCount = 0, emptyIdx = -1;
                    for (int i = 0; i < 5; i++)
                    {
                        int cell = board[rr[i], cc[i]];
                        if (cell == player) playerCount++;
                        else if (cell == 0) { emptyCount++; emptyIdx = i; }
                        else oppCount++;
                    }
                    if (playerCount == 4 && emptyCount == 1 && oppCount == 0 && (emptyIdx == 1 || emptyIdx == 2 || emptyIdx == 3))
                    {
                        int beforeR = r - dr, beforeC = c - dc;
                        int afterR = r + dr * 5, afterC = c + dc * 5;
                        bool beforeOnBoard = (uint)beforeR < (uint)size && (uint)beforeC < (uint)size;
                        bool afterOnBoard = (uint)afterR < (uint)size && (uint)afterC < (uint)size;
                        bool beforeIsPlayer = beforeOnBoard && board[beforeR, beforeC] == player;
                        bool afterIsPlayer = afterOnBoard && board[afterR, afterC] == player;
                        if (!beforeIsPlayer && !afterIsPlayer)
                            fourGap++;
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
        score += WeightLiberty * liberties;

        return score;
    }
}


