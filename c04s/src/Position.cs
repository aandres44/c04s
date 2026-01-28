using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace c04s.src;

/**
* A class storing a Connect 4 position.
* Function are relative to the current player to play.
* Position containing alignment are not supported by this class.
* 
* A binary bitboard representationis used.
* Each column is encoded on HEIGH + 1 bits.
* 
* Example of bit order to encode for a 7x6 board
* .  .  .  .  .  .  .
* 5 12 19 26 33 40 47
* 4 11 18 25 32 39 46
* 3 10 17 24 31 38 45
* 2  9 16 23 30 37 44
* 1  8 15 22 29 36 43
* 0  7 14 21 28 35 42 
* 
* Position is stored as
* - a bitboard "mask" with 1 on any color stones
* - a bitboard "currentPlayer" with 1 on stones of current player
* 
* "currentPlayer" bitboard can be transformed into a compact and non ambiguous key
* by adding an extra bit on top of the last non empty cell of each column.
* This allow to identify all the empty cells whithout needing "mask" bitboard
* 
* currentPlayer "x" = 1, opponent "o" = 0
* board     position  mask      key       bottom
*           0000000   0000000   0000000   0000000
* .......   0000000   0000000   0001000   0000000
* ...o...   0000000   0001000   0010000   0000000
* ..xx...   0011000   0011000   0011000   0000000
* ..ox...   0001000   0011000   0001100   0000000
* ..oox..   0000100   0011100   0000110   0000000
* ..oxxo.   0001100   0011110   1101101   1111111
*
* currentPlayer "o" = 1, opponent "x" = 0
* board     position  mask      key       bottom
*           0000000   0000000   0001000   0000000
* ...x...   0000000   0001000   0000000   0000000
* ...o...   0001000   0001000   0011000   0000000
* ..xx...   0000000   0011000   0000000   0000000
* ..ox...   0010000   0011000   0010100   0000000
* ..oox..   0011000   0011100   0011010   0000000
* ..oxxo.   0010010   0011110   1110011   1111111
*
* key is an unique representation of a board key = position + mask + bottom
* in practice, as bottom is constant, key = position + mask is also a 
* non-ambigous representation of the position.
*/
internal struct Position
{
    public const int WIDTH = 7;  // Width of the board
    public const int HEIGHT = 6; // Height of the board
    public const int MIN_SCORE = -(WIDTH * HEIGHT) / 2 + 3;
    public const int MAX_SCORE = (WIDTH * HEIGHT + 1) / 2 - 3;
    public const int MAX_MOVES = Position.WIDTH * Position.HEIGHT;

    // Static bitmaps
    static readonly ulong BottomMask = Bottom(WIDTH, HEIGHT);
    static readonly ulong BoardMask = BottomMask * ((1UL << HEIGHT) - 1UL);

    private ulong currentPosition;  // bitmap of the current_player stones
    private ulong mask;             // bitmap of all the already palyed
    private uint ply;               // number of half moves played since the beinning of the game

    public Position()
    {
        currentPosition = 0;
        mask = 0;
        ply = 0;
    }
    
    public void Reset()
    {
        currentPosition = 0;
        mask = 0;
        ply = 0;
    }

    /// <summary>
    /// Gets the ulong position
    /// </summary>
    /// <returns>The current position</returns>
    public readonly ulong GetCurrentPlayerPositions()
    {
        return currentPosition;
    }

    /// <summary>
    /// Gets the ulong position
    /// </summary>
    /// <returns>The current position</returns>
    public readonly ulong GetOtherPlayerPositions()
    {
        return currentPosition ^ mask;
    }

    /*
    * Generate a bitmask containing one for the bottom slot of each colum
    */
    public static ulong Bottom(int width, int height)
    {
        return width == 0
        ? 0UL
        : Bottom(width - 1, height) | (1UL << ((width - 1) * (height + 1)));
    }

    /**
     * Indicates whether a column is playable.
     * @param col: 0-based index of column to play
     * @return true if the column is playable, false if the column is already full.
     */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool CanPlay(int col)
    {
        return (mask & TopMaskCol(col)) == 0;
    }

    /**
    * Plays a playable column.
    * This function should not be called on a non-playable column or a column making an alignment.
    *
    * @param col: 0-based index of a playable column.
    */
    void PlayCol(int col)
    {
        Play((mask + BottomMaskCol(col)) & ColumnMask(col));
    }

    /**
     * Plays a playable column.
     * This function should not be called on a non-playable column or a column making an alignment.
     *
     * @param col: 0-based index of a playable column.
     */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Play(ulong move)
    {
        currentPosition ^= mask;
        mask |= move;
        ply++;
        return move;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Undo(ulong move)
    {
        ply--;
        mask ^= move;
        currentPosition ^= mask;
    }

    /*
    * Plays a sequence of successive played columns, mainly used to initilize a board.
    * @param seq: a sequence of digits corresponding to the 1-based index of the column played.
    *
    * @return number of played moves. Processing will stop at first invalid move that can be:
    *           - invalid character (non digit, or digit >= WIDTH)
    *           - playing a colum the is already full
    *           - playing a column that makes an alignment (we only solve non).
    *         Caller can check if the move sequence was valid by comparing the number of 
    *         processed moves to the length of the sequence.
    */
    public uint Play(string seq)
    {
        for (uint i = 0; i < seq.Length; i++)
        {
            int col = seq[(int)i] - '1'; // Substract 1 since the sequence comes on 1-based index
            if (col < 0 || col >= WIDTH || !CanPlay(col) || IsWinningMove(col))
            {
                return i; // invalid move
            }
            PlayCol(col);
        }
        return (uint)seq.Length;
    }

    /*
    * return true if current player can win next move
    */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool CanWinNext()
    {
        return (WinningPosition() & Possible()) != 0;
    }

    /**
     * Indicates whether the current player wins by playing a given column.
     * This function should never be called on a non-playable column.
     * @param col: 0-based index of a playable column.
     * @return true if current player makes an alignment by playing the corresponding column col.
     */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool IsWinningMove(int col)
    {
        return (WinningPosition() & Possible() & ColumnMask(col)) != 0;
    }

    /*
    * Return a bitmask of the possible winning positions for the current player
    */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ulong WinningPosition()
    {
        return ComputeWinningPosition(currentPosition, mask);
    }

    /*
    * Return a bitmask of the possible winning positions for the opponent
    */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ulong OpponentWinningPosition() {
        return ComputeWinningPosition(currentPosition ^ mask, mask);
      }

    /*
    * Bitmap of the next possible valid moves for the current player
    * Including losing moves.
    */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ulong Possible()
    {
        return (mask + BottomMask) & BoardMask;
    }

    /*
    * counts number of bit set to one in a 64bits integer
    */
    public static uint Popcount(ulong m)
    {
        uint c;
        for (c = 0; m != 0; c++) m &= m - 1;
        return c;
    }

    /*
    * @parmam position, a bitmap of the player to evaluate the winning pos
    * @param mask, a mask of the already played spots
    *
    * @return a bitmap of all the winning free spots making an alignment
    */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ulong ComputeWinningPosition(ulong position, ulong mask)
    {
        // vertical
        ulong r = (position << 1) & (position << 2) & (position << 3);

        // horizontal
        ulong p = (position << (HEIGHT + 1)) & (position << (2 * (HEIGHT + 1)));
        r |= p & (position << (3 * (HEIGHT + 1)));
        r |= p & (position >> (HEIGHT + 1));

        p = (position >> (HEIGHT + 1)) & (position >> (2 * (HEIGHT + 1)));
        r |= p & (position << (HEIGHT + 1));
        r |= p & (position >> (3 * (HEIGHT + 1)));

        // diagonal 1
        p = (position << HEIGHT) & (position << (2 * HEIGHT));
        r |= p & (position << (3 * HEIGHT));
        r |= p & (position >> HEIGHT);

        p = (position >> HEIGHT) & (position >> (2 * HEIGHT));
        r |= p & (position << HEIGHT);
        r |= p & (position >> (3 * HEIGHT));

        // diagonal 2
        p = (position << (HEIGHT + 2)) & (position << (2 * (HEIGHT + 2)));
        r |= p & (position << (3 * (HEIGHT + 2)));
        r |= p & (position >> (HEIGHT + 2));

        p = (position >> (HEIGHT + 2)) & (position >> (2 * (HEIGHT + 2)));
        r |= p & (position << (HEIGHT + 2));
        r |= p & (position >> (3 * (HEIGHT + 2)));

        return r & (BoardMask ^ mask);
    }


    /**    
     * @return number of moves played from the beginning of the game.
     */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly uint NbMoves()
    {
        return ply;
    }

    /**    
    * @return a compact representation of a position on WIDTH*(HEIGHT+1) bits.
    */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ulong Key()
    {
        return currentPosition + mask;
    }

    /*
    * Return a bitmap of all the possible next moves the do not lose in one turn.
    * A losing move is a move leaving the possibility for the opponent to win directly.
    *
    * Warning this function is intended to test position where you cannot win in one turn
    * If you have a winning move, this function can miss it and prefer to prevent the opponent
    * to make an alignment.
    */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ulong PossibleNonLosingMoves() 
    {
        Debug.Assert(!CanWinNext());
        ulong possibleMask = Possible();
        ulong opponentWin = OpponentWinningPosition();
        ulong forcedMoves = possibleMask & opponentWin;
        if(forcedMoves != 0)
        {
            if((forcedMoves & (forcedMoves - 1)) != 0) // check if there is more than one forced move
                return 0;                           // the opponnent has two winning moves and you cannot stop him
            else possibleMask = forcedMoves;    // enforce to play the single forced move
        }
        return possibleMask & ~(opponentWin >> 1);  // avoid to play below an opponent winning spot
    }

    /**
    * Score a possible move.
    *
    * @param move, a possible move given in a bitmap format.
    *
    * The score we are using is the number of winning spots
    * the current player has after playing the move.
    */
    public readonly int MoveScore(ulong move)
    {
        return (int)Popcount(ComputeWinningPosition(currentPosition | move, mask));
    }

    // return a bitmask containg a single 1 corresponding to the top cel of a given column
    private static ulong TopMaskCol(int col)
    {
        return ((ulong)(1) << (HEIGHT - 1)) << col * (HEIGHT + 1);
    }

    // return a bitmask containg a single 1 corresponding to the bottom cell of a given column
    private static ulong BottomMaskCol(int col)
    {
        return (ulong)(1) << col * (HEIGHT + 1);
    }

    // return a bitmask 1 on all the cells of a given column
    public static ulong ColumnMask(int col)
    {
        return (((ulong)(1) << HEIGHT) - 1) << col * (HEIGHT + 1);
    }
}
