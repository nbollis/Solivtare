namespace SolvitaireCore.ConnectFour;

public class ConnectFourHeuristicEvaluator : StateEvaluator<ConnectFourGameState, ConnectFourMove>
{
    public override double EvaluateMove(ConnectFourGameState state, ConnectFourMove move)
    {
        state.ExecuteMove(move);
        double score = EvaluateState(state, 3 - state.CurrentPlayer);
        state.UndoMove(move);
        return score;
    }

    public override double EvaluateState(ConnectFourGameState state, int? maximixingPlayerId = null)
    {
        maximixingPlayerId ??= state.CurrentPlayer;
        if (state.IsPlayerWin(maximixingPlayerId!.Value))
            return 1000;
        if (state.IsPlayerLoss(maximixingPlayerId.Value))
            return -1000;

        return EvaluateHeuristic(state, maximixingPlayerId.Value) - EvaluateHeuristic(state, 3 - maximixingPlayerId.Value);
    }

    private int EvaluateHeuristic(ConnectFourGameState state, int player)
    {
        int score = 0;

        // Favor center column  
        for (int row = 0; row < ConnectFourGameState.Rows; row++)
        {
            if (state.Board[row, 3] == player)
                score += 3;
        }

        // Evaluate rows, columns, and diagonals for 2- and 3-in-a-rows  
        for (int row = 0; row < ConnectFourGameState.Rows; row++)
        {
            for (int col = 0; col < ConnectFourGameState.Columns; col++)
            {
                // Horizontal check  
                if (col <= ConnectFourGameState.Columns - 4)
                    score += EvaluateLine(state, player, row, col, 0, 1);

                // Vertical check  
                if (row <= ConnectFourGameState.Rows - 4)
                    score += EvaluateLine(state, player, row, col, 1, 0);

                // Diagonal (down-right) check  
                if (row <= ConnectFourGameState.Rows - 4 && col <= ConnectFourGameState.Columns - 4)
                    score += EvaluateLine(state, player, row, col, 1, 1);

                // Diagonal (up-right) check  
                if (row >= 3 && col <= ConnectFourGameState.Columns - 4)
                    score += EvaluateLine(state, player, row, col, -1, 1);
            }
        }

        return score;
    }

    private int EvaluateLine(ConnectFourGameState state, int player, int startRow, int startCol, int dRow, int dCol)
    {
        int count = 0;
        for (int i = 0; i < 4; i++)
        {
            int row = startRow + i * dRow;
            int col = startCol + i * dCol;
            if (state.Board[row, col] == player)
                count++;
            else if (state.Board[row, col] != 0)
                return 0; // Blocked by opponent  
        }

        if (count == 2) return 2;
        if (count == 3) return 5;
        return 0;
    }
}