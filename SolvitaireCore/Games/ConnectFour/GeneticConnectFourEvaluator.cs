using System.Runtime.CompilerServices;

namespace SolvitaireCore.ConnectFour;

public class GeneticConnectFourEvaluator : StateEvaluator<ConnectFourGameState, ConnectFourMove>
{
    private static readonly (int dr, int dc)[] Directions = new[] { (0, 1), (1, 0), (1, 1), (1, -1) };
    private readonly ConnectFourChromosome _chromosome;

    // Cache weights as readonly fields  
    private readonly double _isolatedPieceWeight;
    private readonly double _twoInARowWeight;
    private readonly double _twoWithOneGapWeight;
    private readonly double _threeInARowWeight;
    private readonly double _threeWithOneGapWeight;
    private readonly double _surroundedPieceWeight;
    private readonly double _playerTouchingWeight;
    private readonly double _opponentTouchingWeight;

    // Move weights  
    private readonly double[] _columnWeights;
    private readonly double[] _rowWeights;

    public GeneticConnectFourEvaluator(ConnectFourChromosome chromosome)
    {
        _chromosome = chromosome;
        _isolatedPieceWeight = chromosome.GetWeight(ConnectFourChromosome.IsolatedPieceWeight);
        _twoInARowWeight = chromosome.GetWeight(ConnectFourChromosome.TwoInARowWeight);
        _twoWithOneGapWeight = chromosome.GetWeight(ConnectFourChromosome.TwoWithOneGapWeight);
        _threeInARowWeight = chromosome.GetWeight(ConnectFourChromosome.ThreeInARowWeight);
        _threeWithOneGapWeight = chromosome.GetWeight(ConnectFourChromosome.ThreeWithOneGapWeight);
        _surroundedPieceWeight = chromosome.GetWeight(ConnectFourChromosome.SurroundedPieceWeight);
        _playerTouchingWeight = chromosome.GetWeight(ConnectFourChromosome.PlayerTouchingWeight);
        _opponentTouchingWeight = chromosome.GetWeight(ConnectFourChromosome.OpponentTouchingWeight);

        // Cache column weights  
        _columnWeights = new double[ConnectFourGameState.Columns];
        _columnWeights[0] = chromosome.GetWeight(ConnectFourChromosome.ColumnOneWeight);
        _columnWeights[1] = chromosome.GetWeight(ConnectFourChromosome.ColumnTwoWeight);
        _columnWeights[2] = chromosome.GetWeight(ConnectFourChromosome.ColumnThreeWeight);
        _columnWeights[3] = chromosome.GetWeight(ConnectFourChromosome.ColumnFourWeight);
        _columnWeights[4] = chromosome.GetWeight(ConnectFourChromosome.ColumnFiveWeight);
        _columnWeights[5] = chromosome.GetWeight(ConnectFourChromosome.ColumnSixWeight);
        _columnWeights[6] = chromosome.GetWeight(ConnectFourChromosome.ColumnSevenWeight);

        // Cache row weights  
        _rowWeights = new double[ConnectFourGameState.Rows];
        _rowWeights[0] = chromosome.GetWeight(ConnectFourChromosome.RowOneWeight);
        _rowWeights[1] = chromosome.GetWeight(ConnectFourChromosome.RowTwoWeight);
        _rowWeights[2] = chromosome.GetWeight(ConnectFourChromosome.RowThreeWeight);
        _rowWeights[3] = chromosome.GetWeight(ConnectFourChromosome.RowFourWeight);
        _rowWeights[4] = chromosome.GetWeight(ConnectFourChromosome.RowFiveWeight);
        _rowWeights[5] = chromosome.GetWeight(ConnectFourChromosome.RowSixWeight);
    }

    public override double EvaluateMove(ConnectFourGameState state, ConnectFourMove move)
    {
        // Evaluate the move based on the column and row it would land in  
        int row = GetLandingRow(state, move.Column);
        double score = 0;

        // Column-based weights  
        score += _columnWeights[move.Column];

        // Row-based weights  
        score += _rowWeights[row];

        // Add more move-based features here
        return score;
    }

    public override double EvaluateState(ConnectFourGameState state, int? maximizingPlayerId = null)
    {
        maximizingPlayerId ??= state.CurrentPlayer;
        int player = maximizingPlayerId.Value;
        int opponent = 3 - player;

        // Cache board and dimensions for efficiency
        int[,] board = state.Board;
        int rows = ConnectFourGameState.Rows;
        int cols = ConnectFourGameState.Columns;

        // Precompute all features in a single board scan
        int playerIsolated = 0, opponentIsolated = 0;
        int playerSurrounded = 0, opponentSurrounded = 0;
        int playerTwo = 0, playerTwoGap = 0, playerThree = 0, playerThreeGap = 0;
        int opponentTwo = 0, opponentTwoGap = 0, opponentThree = 0, opponentThreeGap = 0;
        int playerTouching = 0, playerTouchingOpponent = 0;
        int opponentTouching = 0, opponentTouchingPlayer = 0;

        // Scan the board once, extracting all features for both players
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                int cell = board[row, col];
                if (cell == 0) continue; // Skip empty cells

                // --- Neighbor analysis for isolated, surrounded, and touching features ---
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
                            // Edge of the board counts as non-empty for "surrounded"
                            totalNeighbors++;
                            nonEmptyNeighbors++;
                        }
                    }
                }

                // Count how many of your own and opponent pieces this piece is touching
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

                // Isolated: no same-player neighbors
                if (sameNeighbors == 0)
                {
                    if (cell == player) playerIsolated++;
                    else opponentIsolated++;
                }

                // Surrounded: all adjacent cells (or edges) are non-empty
                if (nonEmptyNeighbors == totalNeighbors)
                {
                    if (cell == player) playerSurrounded++;
                    else opponentSurrounded++;
                }

                // --- N-in-a-row and N-with-gap feature extraction ---
                foreach (var (dr, dc) in Directions)
                {
                    // 2-in-a-row: count all overlapping pairs
                    int r1 = row + dr, c1 = col + dc;
                    if ((uint)r1 < (uint)rows && (uint)c1 < (uint)cols)
                    {
                        if (cell == player && board[r1, c1] == player)
                            playerTwo++;
                        if (cell == opponent && board[r1, c1] == opponent)
                            opponentTwo++;
                    }

                    // 3-in-a-row: count all overlapping triplets
                    int r2 = row + 2 * dr, c2 = col + 2 * dc;
                    if ((uint)r1 < (uint)rows && (uint)c1 < (uint)cols &&
                        (uint)r2 < (uint)rows && (uint)c2 < (uint)cols)
                    {
                        if (cell == player && board[r1, c1] == player && board[r2, c2] == player)
                            playerThree++;
                        if (cell == opponent && board[r1, c1] == opponent && board[r2, c2] == opponent)
                            opponentThree++;
                    }

                    // 2-with-gap and 3-with-gap (use 4-cell window, as before)
                    Span<int> window = stackalloc int[4];
                    bool fullWindow = true;
                    int wr = row, wc = col;
                    for (int i = 0; i < 4; i++, wr += dr, wc += dc)
                    {
                        if ((uint)wr >= (uint)rows || (uint)wc >= (uint)cols)
                        {
                            fullWindow = false;
                            break;
                        }
                        window[i] = Unsafe.Add(ref board[wr, wc], 0);
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
                        // Player
                        if (playerCount == 2 && opponentCount == 0 && emptyCount == 2 && HasGapWindow(window, player, 2))
                            playerTwoGap++;
                        if (playerCount == 3 && opponentCount == 0 && emptyCount == 1 && HasGapWindow(window, player, 3))
                            playerThreeGap++;
                        // Opponent
                        if (opponentCount == 2 && playerCount == 0 && emptyCount == 2 && HasGapWindow(window, opponent, 2))
                            opponentTwoGap++;
                        if (opponentCount == 3 && playerCount == 0 && emptyCount == 1 && HasGapWindow(window, opponent, 3))
                            opponentThreeGap++;
                    }
                }
            }
        }

        // --- Weighted sum of all features for both players ---
        // Player features are positive, opponent features are negative (adversarial)
        double score = 0;
        score += _isolatedPieceWeight * playerIsolated;
        score += _twoInARowWeight * playerTwo;
        score += _twoWithOneGapWeight * playerTwoGap;
        score += _threeInARowWeight * playerThree;
        score += _threeWithOneGapWeight * playerThreeGap;
        score += _surroundedPieceWeight * playerSurrounded;
        score += _playerTouchingWeight * playerTouching;
        score += _opponentTouchingWeight * playerTouchingOpponent;

        score -= _isolatedPieceWeight * opponentIsolated;
        score -= _twoInARowWeight * opponentTwo;
        score -= _twoWithOneGapWeight * opponentTwoGap;
        score -= _threeInARowWeight * opponentThree;
        score -= _threeWithOneGapWeight * opponentThreeGap;
        score -= _surroundedPieceWeight * opponentSurrounded;
        score -= _playerTouchingWeight * opponentTouching;
        score -= _opponentTouchingWeight * opponentTouchingPlayer;

        return score;
    }

    /// <summary>
    /// Checks for a gap in a 4-cell window: returns true if there are n pieces, 4-n empty,
    /// and the pieces are not all consecutive (i.e., there is at least one empty cell between them).
    /// </summary>
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

    /// <summary>
    /// Returns the row index where a piece would land if dropped in the given column.
    /// </summary>
    private int GetLandingRow(ConnectFourGameState state, int column)
    {
        for (int row = ConnectFourGameState.Rows - 1; row >= 0; row--)
        {
            if (state.Board[row, column] == 0)
                return row;
        }
        return 0; // Should not happen if move is legal
    }
}
