using SolvitaireCore;
using SolvitaireCore.ConnectFour;

namespace SolvitaireGenetics;

public class GeneticConnectFourEvaluator : StateEvaluator<ConnectFourGameState, ConnectFourMove>
{
    private readonly ConnectFourChromosome _chromosome;

    public GeneticConnectFourEvaluator(ConnectFourChromosome chromosome)
    {
        _chromosome = chromosome;
    }

    public override double EvaluateMove(ConnectFourGameState state, ConnectFourMove move)
    {
        // Evaluate the move based on the column and row it would land in
        int row = GetLandingRow(state, move.Column);
        double score = 0;

        // Column-based weights
        switch (move.Column)
        {
            case 0: score += _chromosome.GetWeight(ConnectFourChromosome.ColumnOneWeight); break;
            case 1: score += _chromosome.GetWeight(ConnectFourChromosome.ColumnTwoWeight); break;
            case 2: score += _chromosome.GetWeight(ConnectFourChromosome.ColumnThreeWeight); break;
            case 3: score += _chromosome.GetWeight(ConnectFourChromosome.ColumnFourWeight); break;
            case 4: score += _chromosome.GetWeight(ConnectFourChromosome.ColumnFiveWeight); break;
            case 5: score += _chromosome.GetWeight(ConnectFourChromosome.ColumnSixWeight); break;
            case 6: score += _chromosome.GetWeight(ConnectFourChromosome.ColumnSevenWeight); break;
        }

        // Row-based weights
        switch (row)
        {
            case 0: score += _chromosome.GetWeight(ConnectFourChromosome.RowOneWeight); break;
            case 1: score += _chromosome.GetWeight(ConnectFourChromosome.RowTwoWeight); break;
            case 2: score += _chromosome.GetWeight(ConnectFourChromosome.RowThreeWeight); break;
            case 3: score += _chromosome.GetWeight(ConnectFourChromosome.RowFourWeight); break;
            case 4: score += _chromosome.GetWeight(ConnectFourChromosome.RowFiveWeight); break;
            case 5: score += _chromosome.GetWeight(ConnectFourChromosome.RowSixWeight); break;
        }

        // Optionally, you can add more move-based features here

        return score;
    }

    public override double EvaluateState(ConnectFourGameState state, int? maximizingPlayerId = null)
    {
        maximizingPlayerId ??= state.CurrentPlayer;
        int player = maximizingPlayerId.Value;
        int opponent = 3 - player;

        double score = 0;

        // Count features for the current player
        score += _chromosome.GetWeight(ConnectFourChromosome.IsolatedPieceWeight) * CountIsolatedPieces(state, player);
        score += _chromosome.GetWeight(ConnectFourChromosome.TwoInARowWeight) * CountNInARow(state, player, 2, false);
        score += _chromosome.GetWeight(ConnectFourChromosome.TwoWithOneGapWeight) * CountNInARow(state, player, 2, true);
        score += _chromosome.GetWeight(ConnectFourChromosome.ThreeInARowWeight) * CountNInARow(state, player, 3, false);
        score += _chromosome.GetWeight(ConnectFourChromosome.ThreeWithOneGapWeight) * CountNInARow(state, player, 3, true);
        score += _chromosome.GetWeight(ConnectFourChromosome.SurroundedPieceWeight) * CountSurroundedPieces(state, player);

        // Optionally, subtract opponent's features (for adversarial evaluation)
        return score;
    }

    private int GetLandingRow(ConnectFourGameState state, int column)
    {
        for (int row = ConnectFourGameState.Rows - 1; row >= 0; row--)
        {
            if (state.Board[row, column] == 0)
                return row;
        }
        return 0; // Should not happen if move is legal
    }

    private int CountIsolatedPieces(ConnectFourGameState state, int player)
    {
        int count = 0;
        for (int row = 0; row < ConnectFourGameState.Rows; row++)
        {
            for (int col = 0; col < ConnectFourGameState.Columns; col++)
            {
                if (state.Board[row, col] == player)
                {
                    bool isolated = true;
                    for (int dr = -1; dr <= 1; dr++)
                    {
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            if (dr == 0 && dc == 0) continue;
                            int nr = row + dr, nc = col + dc;
                            if (nr >= 0 && nr < ConnectFourGameState.Rows && nc >= 0 && nc < ConnectFourGameState.Columns)
                            {
                                if (state.Board[nr, nc] == player)
                                {
                                    isolated = false;
                                    break;
                                }
                            }
                        }
                        if (!isolated) break;
                    }
                    if (isolated) count++;
                }
            }
        }
        return count;
    }

    private int CountNInARow(ConnectFourGameState state, int player, int n, bool withGap)
    {
        int count = 0;
        // Horizontal, vertical, and both diagonals
        int[][] directions = new[] { new[] { 0, 1 }, new[] { 1, 0 }, new[] { 1, 1 }, new[] { 1, -1 } };
        for (int row = 0; row < ConnectFourGameState.Rows; row++)
        {
            for (int col = 0; col < ConnectFourGameState.Columns; col++)
            {
                foreach (var dir in directions)
                {
                    int dr = dir[0], dc = dir[1];
                    int playerCount = 0, emptyCount = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        int nr = row + dr * i, nc = col + dc * i;
                        if (nr < 0 || nr >= ConnectFourGameState.Rows || nc < 0 || nc >= ConnectFourGameState.Columns)
                            break;
                        if (state.Board[nr, nc] == player)
                            playerCount++;
                        else if (state.Board[nr, nc] == 0)
                            emptyCount++;
                    }
                    if (withGap)
                    {
                        if (playerCount == n && emptyCount == 4 - n)
                            count++;
                    }
                    else
                    {
                        if (playerCount == n && emptyCount == 4 - n && !HasGap(state, row, col, dr, dc, n, player))
                            count++;
                    }
                }
            }
        }
        return count;
    }

    private bool HasGap(ConnectFourGameState state, int row, int col, int dr, int dc, int n, int player)
    {
        // Checks if the n-in-a-row is consecutive (no gap)
        int consecutive = 0;
        for (int i = 0; i < 4; i++)
        {
            int nr = row + dr * i, nc = col + dc * i;
            if (nr < 0 || nr >= ConnectFourGameState.Rows || nc < 0 || nc >= ConnectFourGameState.Columns)
                return true;
            if (state.Board[nr, nc] == player)
                consecutive++;
            else if (consecutive > 0)
                return true; // Found a gap
        }
        return false;
    }

    private int CountSurroundedPieces(ConnectFourGameState state, int player)
    {
        int count = 0;
        for (int row = 0; row < ConnectFourGameState.Rows; row++)
        {
            for (int col = 0; col < ConnectFourGameState.Columns; col++)
            {
                if (state.Board[row, col] == player)
                {
                    int neighborCount = 0;
                    for (int dr = -1; dr <= 1; dr++)
                    {
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            if (dr == 0 && dc == 0) continue;
                            int nr = row + dr, nc = col + dc;
                            if (nr >= 0 && nr < ConnectFourGameState.Rows && nc >= 0 && nc < ConnectFourGameState.Columns)
                            {
                                if (state.Board[nr, nc] != 0)
                                    neighborCount++;
                            }
                        }
                    }
                    // You can adjust the threshold (e.g., 4 or more neighbors)
                    if (neighborCount >= 4)
                        count++;
                }
            }
        }
        return count;
    }

}
