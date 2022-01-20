using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlipConnectFour
{
    public class Board
    {
        public enum State
        {
            Empty,
            One,
            Two
        }
        public int Width { get; }
        public int Height { get; }
        public int WinAmount { get; }
        // First move can never be a flip
        public bool CanFlip { get; private set; } = false;
        // -1 is flip
        public List<int> Actions { get; }

        private State[,] board;
        private int[] insertions;

        public Board(int w, int h, int wa)
        {
            Width = w;
            Height = h;
            board = new State[w,h];
            insertions = new int[w];
            WinAmount = wa;
            Actions = new();
        }
        public Board(Board other)
        {
            Width = other.Width;
            Height = other.Height;
            board = other.GameBoard();
            insertions = (other.insertions.Clone() as int[])!;
            WinAmount = other.WinAmount;
            CanFlip = other.CanFlip;
            Actions = new(other.Actions);
        }
        private int ScanForWin(State target, int col, int row)
        {
            return Math.Max(Math.Max(ScanForWinHorizontal(target, col, row), ScanForWinVertical(target, col, row)), ScanForWinDiagonal(target, col, row));
        }
        private int ScanForWinHorizontal(State target, int col, int row)
        {
            int count = 0;
            // Horizontal Check
            for (int i = col; i >= 0; i--)
            {
                if (board[i, row] == target)
                {
                    count++;
                } else
                {
                    break;
                }
            }
            for (int i = col + 1; i < Width; i++)
            {
                if (board[i, row] == target)
                {
                    count++;
                } else
                {
                    break;
                }
            }
            return count;
        }
        private int ScanForWinVertical(State target, int col, int row)
        {
            int count = 0;
            // Vertical
            for (int i = row; i >= 0; i--)
            {
                if (board[col, i] == target)
                {
                    count++;
                } else
                {
                    break;
                }
            }
            for (int i = row + 1; i < Height; i++)
            {
                if (board[col, i] == target)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            return count;
        }
        private int ScanForWinDiagonal(State target, int col, int row)
        {
            int count = 0;
            int i = col;
            int j = row;
            while (i >= 0 && j >= 0)
            {
                if (board[i--, j--] == target)
                {
                    count++;
                } else
                {
                    break;
                }
            }
            i = col + 1;
            j = row + 1;
            while (i < Width && j < Height)
            {
                if (board[i++, j++] == target)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            // Check other diagonal
            int count2 = 0;
            i = col;
            j = row;
            while (i >= 0 && j < Height)
            {
                if (board[i--, j++] == target)
                {
                    count2++;
                }
                else
                {
                    break;
                }
            }
            i = col + 1;
            j = row - 1;
            while (i < Width && j >= 0)
            {
                if (board[i++, j--] == target)
                {
                    count2++;
                }
                else
                {
                    break;
                }
            }
            return Math.Max(count, count2);
        }
        private int Place(State[,] board, State st, int col)
        {
            int insertLoc = insertions[col];
            board[col, insertLoc] = st;
            insertions[col]++;
            return ScanForWin(st, col, insertLoc);
        }
        public State[,] GameBoard()
        {
            return (board.Clone() as State[,])!;
        }
        
        public bool CanPlace(int col)
        {
            return insertions[col] < Height;
        }
        /// <summary>
        /// Places a piece at the specified column. Returns true if a win is obtained by this move.
        /// </summary>
        /// <param name="st"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool Place(State st, int col)
        {
            if (col < 0 || col > Width)
            {
                throw new ArgumentException("Cannot place a piece in an invalid column!", nameof(col));
            }
            if (insertions[col] >= Height)
            {
                throw new ArgumentException("Cannot place a piece in a full column!");
            }
            // After a public placement, flips are allowed again
            CanFlip = true;
            Actions.Add(col);
            return Place(board, st, col) >= WinAmount;
        }
        public void Reset()
        {
            board = new State[Width, Height];
            insertions = new int[Width];
        }
        /// <summary>
        /// Flips the board. Returns the <see cref="State"/> for who won, or <see cref="State.Empty"/> if neither player won or in the case of a perfect draw.
        /// </summary>
        /// <returns></returns>
        public State Flip()
        {
            if (!CanFlip)
                throw new InvalidOperationException("Cannot flip!");
            // Disallow consecutive flips
            CanFlip = false;
            Actions.Add(-1);
            var oldBoard = board;
            var oldInsertions = insertions;
            var winLength = new Dictionary<State, int>();
            Reset();

            for (int i = 0; i < Width; i++)
            {
                // For each col
                for (int j = oldInsertions[i] - 1; j >= 0; j--)
                {
                    // For each piece in REVERSE order, place it into the new board.
                    State player = oldBoard[i, j];
                    if (player == State.Empty)
                    {
                        throw new InvalidOperationException("Floating board?");
                    }
                    var res = Place(board, player, i);
                    if (winLength.TryGetValue(player, out var winCount))
                    {
                        if (res > winCount)
                        {
                            winLength[player] = res;
                        }
                    } else
                    {
                        winLength[player] = res;
                    }
                }
            }
            // Board should be complete at this point.
            int bestWin = 0;
            State bestPlayer = State.Empty;
            int bestWinCount = 0;
            foreach ((State player, int val) in winLength)
            {
                if (val < WinAmount)
                    // Skip connections that are too low to trigger a win
                    continue;
                if (val > bestWin)
                {
                    bestWin = val;
                    bestWinCount = 1;
                    bestPlayer = player;
                }
                else if (val == bestWin)
                {
                    bestWinCount++;
                }
            }
            // If there are at least two ties for first, it's a tie
            // If there is a clear winner, they win.
            // Note that this does NOT fallback to a (potentially?) useful win check condition which would be checking the number of wins
            // This may need to be changed?
            return bestWinCount == 1 ? bestPlayer : State.Empty;
        }
        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (var act in Actions)
            {
                sb.Append(act.ToString() + " ");
            }
            sb.AppendLine();
            for (int i = Height - 1; i >= 0; i--)
            {
                for (int j = 0; j < Width; j++)
                {
                    sb.Append(board[j, i] + " ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}