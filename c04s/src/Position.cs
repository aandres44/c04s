using System;

namespace c04s.src;

/**
 * A class storing a Connect 4 position.
 * Function are relative to the current player to play.
 * Position containing aligment are not supported by this class.
 */
internal class Position : ICloneable
{
    public const int WIDTH = 7;  // Width of the board
    public const int HEIGHT = 6; // Height of the board

    private int[,] board; // 0 if cell is empty, 1 for first player and 2 for second player.
    private int[] height; // number of stones per column
    private uint ply;     // number of half moves played since the beinning of the game.

    public Position()
    {
        board = new int[WIDTH, HEIGHT];
        height = new int[WIDTH];
        ply = 0;
    }


    /**
     * Indicates whether a column is playable.
     * @param col: 0-based index of column to play
     * @return true if the column is playable, false if the column is already full.
     */
    public bool CanPlay(int col)
    {
        return height[col] < HEIGHT;
    }

    /**
     * Plays a playable column.
     * This function should not be called on a non-playable column or a column making an alignment.
     *
     * @param col: 0-based index of a playable column.
     */
    public void Play(int col)
    {
        board[col, height[col]] = (int)(1 + ply%2); // Sets the current player
        height[col]++;
        ply++;
    }

    /*
    * Plays a sequence of successive played columns, mainly used to initilize a board.
    * @param seq: a sequence of digits corresponding to the 1-based index of the column played.
    *
    * @return number of played moves. Processing will stop at first invalid move that can be:
    *           - invalid character (non digit, or digit >= WIDTH)
    *           - playing a colum the is already full
    *           - playing a column that makes an aligment (we only solve non).
    *         Caller can check if the move sequence was valid by comparing the number of 
    *         processed moves to the length of the sequence.
    */
    public uint Play(string seq)
    {
        for (uint i = 0; i < seq.Length; i++)
        {
            int col = seq[(int)i] - '1'; // Substract 1 since the sequence comes on 1-based index
            if (col < 0 || col >= WIDTH || !CanPlay(col) || IsWinningMove(col)) {
                return i; // invalid move
            }
            Play(col);
        }
        return (uint)seq.Length;
    }

    /**
     * Indicates whether the current player wins by playing a given column.
     * This function should never be called on a non-playable column.
     * @param col: 0-based index of a playable column.
     * @return true if current player makes an alignment by playing the corresponding column col.
     */
    public bool IsWinningMove(int col)
    {
        int currentPlayer = 1 + (int)ply % 2;
        // check for vertical alignments
        if (height[col] >= 3
            && board[col,height[col] - 1] == currentPlayer
            && board[col,height[col] - 2] == currentPlayer
            && board[col,height[col] - 3] == currentPlayer)
        {
            return true;
        }
        for (int dy = -1; dy <= 1; dy++) // Iterate on horizontal (dy = 0) or two diagonal directions (dy = -1 or dy = 1).
        {
            int streak = 0; // counter of the number of stones of current player surronding the played stone in tested direction.
            for (int dx = -1; dx <= 1; dx += 2) // count continuous stones of current player on the left, then right of the played column.
            {
                for (int x = col + dx, y = height[col] + dx * dy; x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT && board[x, y] == currentPlayer; streak++)
                {
                    x += dx;
                    y += dx * dy;
                }
            }
            if (streak >= 3) // there is an aligment if at least 3 other stones of the current user 
            {               // are surronding the played stone in the tested direction.
                return true;
            }
        }
        return false;
    }

    /**    
     * @return number of moves played from the beginning of the game.
     */
    public uint NbMoves()
    {
        return ply;
    }

    /// <summary>
    /// Gets the current value of the board in the Position
    /// </summary>
    /// <returns>The board representing the Position</returns>
    public int[,] GetBoard()
    {
        return board;
    }

    /// <summary>
    /// Implements the System.ICloneable Interface
    /// </summary>
    /// <returns>Deep copy of the current position</returns>
    public object Clone()
    {
        return new Position
        {
            board = board.Clone() as int[,] ?? new int[WIDTH,HEIGHT],
            height = height.Clone() as int[] ?? new int[WIDTH],
            ply = this.ply
        };
    }
}
