using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolvitaireCore.ConnectFour;

public class ConnectFourGameState : ITwoPlayerGameState<ConnectFourMove>, IEquatable<ConnectFourGameState>
{
    public const int Rows = 6;

    public const int Columns = 7;

    // 0 = empty, 1 = player1, 2 = player2
    public int[,] Board { get; private set; } = new int[Rows, Columns]; 
    public int CurrentPlayer { get; private set; } = 1;
    public int MovesMade { get; private set; } = 0;

    public int? WinningPlayer { get; private set; } = null;

    public bool IsGameWon { get; private set; }
    public bool IsGameDraw => !IsGameWon && !GetLegalMoves().Any();
    public bool IsGameLost => false; // Or implement if you want to track explicit losses

    public bool IsPlayerWin(int player) => IsGameWon && WinningPlayer == player;
    public bool IsPlayerLoss(int player) => IsGameWon && WinningPlayer != player;


    public void Reset()
    {
        Board = new int[Rows, Columns];
        CurrentPlayer = 1;
        MovesMade = 0;
        IsGameWon = false;
    }

    public List<ConnectFourMove> GetLegalMoves()
    {
        var moves = new List<ConnectFourMove>();
        for (int col = 0; col < Columns; col++)
        {
            if (Board[0, col] == 0)
                moves.Add(new ConnectFourMove(col));
        }
        return moves;
    }

    public void ExecuteMove(ConnectFourMove move)
    {
        for (int row = Rows - 1; row >= 0; row--)
        {
            if (Board[row, move.Column] == 0)
            {
                Board[row, move.Column] = CurrentPlayer;
                MovesMade++;
                if (CheckWin(row, move.Column))
                {
                    IsGameWon = true;
                    WinningPlayer = CurrentPlayer;
                }
                CurrentPlayer = 3 - CurrentPlayer; // toggle between 1 and 2
                return;
            }
        }
        throw new InvalidOperationException("Invalid move: column full");
    }

    public void UndoMove(ConnectFourMove move)
    {
        for (int row = 0; row < Rows; row++)
        {
            if (Board[row, move.Column] != 0)
            {
                Board[row, move.Column] = 0;
                MovesMade--;
                CurrentPlayer = 3 - CurrentPlayer;
                IsGameWon = false;
                return;
            }
        }
        throw new InvalidOperationException("Invalid undo: column empty");
    }

    public IGameState<ConnectFourMove> Clone()
    {
        var clone = new ConnectFourGameState
        {
            Board = (int[,])Board.Clone(),
            CurrentPlayer = CurrentPlayer,
            MovesMade = MovesMade,
            IsGameWon = IsGameWon
        };
        return clone;
    }

    private bool CheckWin(int row, int col)
    {
        int player = Board[row, col];
        return CheckDirection(row, col, 1, 0, player) || // vertical
               CheckDirection(row, col, 0, 1, player) || // horizontal
               CheckDirection(row, col, 1, 1, player) || // diagonal /
               CheckDirection(row, col, 1, -1, player);  // diagonal \
    }

    private bool CheckDirection(int row, int col, int dRow, int dCol, int player)
    {
        int count = 1;
        count += CountDirection(row, col, dRow, dCol, player);
        count += CountDirection(row, col, -dRow, -dCol, player);
        return count >= 4;
    }

    private int CountDirection(int row, int col, int dRow, int dCol, int player)
    {
        int count = 0;
        for (int r = row + dRow, c = col + dCol;
             r >= 0 && r < Rows && c >= 0 && c < Columns && Board[r, c] == player;
             r += dRow, c += dCol)
        {
            count++;
        }
        return count;
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
        int hash = 17;
        foreach (var cell in Board)
            hash = hash * 31 + cell.GetHashCode();
        hash = hash * 31 + CurrentPlayer;
        return hash;
    }
}