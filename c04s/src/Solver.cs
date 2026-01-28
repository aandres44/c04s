using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace c04s.src;

[SkipLocalsInit]

/**
 * A class to solve Connect 4 position using Nagemax variant of min-max algorithm.
 */
internal class Solver
{
    private ulong nodeCount; // counter of explored nodes.

    private readonly int[] columnOrder;

    const int transpositionTableSizeMB = 64;

    private readonly TranspositionTable transTable;

    public Solver()
    {
        nodeCount = 0;
        transTable = new(transpositionTableSizeMB);
        columnOrder = new int[Position.WIDTH];
        for (int i = 0; i < Position.WIDTH; i++)
        {
            columnOrder[i] = Position.WIDTH / 2 + (1 - 2 * (i % 2)) * (i + 1) / 2; // initialize the column exploration order, starting with center columns
        }
    }

    /// <summary>
    /// Recursively solve a connect 4 position using negamax variant of min-max algorithm.    
    /// </summary>
    /// <param name="P">Position</param>
    /// <returns>Score of a position:<br></br>
    /// - 0 for a draw game<br></br>
    /// - Positive score if you can win whatever your opponent is playing.<br></br>
    ///   Your score is the number of moves before the end you can win (the faster you win, the higher your score)<br></br>
    /// - Negative score if your opponent can force you to lose.<br></br>
    ///   Your score is the oposite of the number of moves before the end you will lose (the faster you lose, the lower your score).<br></br>
    /// </returns>
    public sbyte Negamax(ref Position P, int alpha, int beta) {
        Trace.Assert(alpha < beta);
        nodeCount++; // increment counter of explored nodes
        sbyte bestMove = -1;

        if (P.NbMoves() == Position.MAX_MOVES) // check for draw game
        {
            return 0;
        }

        for (int x = 0; x < Position.WIDTH; x++) // check if current player can win next move
        {
            if (P.CanPlay(x) && P.IsWinningMove(x))
            {
                return (sbyte)((Position.MAX_MOVES + 1 - (int) P.NbMoves())/2);
            }
        }

        int max = (Position.MAX_MOVES - 1 - (int) P.NbMoves()) / 2;   // upper bound of our score as we cannot win immediately

        int val = transTable.Get(P.Key());
        if (val != 0)
        {
            max = val + Position.MIN_SCORE - 1;
        }

        if (beta > max)
        {
            beta = max;          // there is no need to keep beta above our max possible score.
            if (alpha >= beta)  // prune the exploration if the [alpha;beta] window is empty.
            {
                return (sbyte)beta;
            }
        }

        for (int x = 0; x < Position.WIDTH; x++) // compute the score of all possible next move and keep the best one
        {
            if (P.CanPlay(columnOrder[x]))
            {
                ulong move = P.Play(columnOrder[x]);               // It's opponent turn in P2 position after current player plays x column.
                int score = -Negamax(ref P, -beta, -alpha); // explore opponent's score within [-beta;-alpha] windows:
                P.Undo(move); // We use Do/Undo instead of Cloning since it is very expensive to do so in C# (.Net)
                // no need to have good precision for score better than beta (opponent's score worse than -beta)
                // no need to check for score worse than alpha (opponent's score worse better than -alpha)
                if (score >= beta) // prune the exploration if we find a possible move better than what we were looking for.
                {
                    return (sbyte)score;
                }
                if (score > alpha) // reduce the [alpha;beta] window for next exploration, as we only 
                {                  // need to search for a position that is better than the best so far.
                    alpha = score;
                    bestMove = (sbyte)move;
                }
            }
        }
        transTable.Put(P.Key(), (byte)(alpha - Position.MIN_SCORE + 1), bestMove); // save the upper bound of the position
        return (sbyte)alpha;
    }

    /// <summary>
    /// Initialize the stack for Negamax
    /// </summary>
    /// <param name="P"></param>
    /// <returns>The final score of all posible nodes</returns>
    public int Solve(Position P, bool weak = false)
    {
        nodeCount = 0; // Relocate this since on Solver instanciation we already set to 0
        int min = (int)-(Position.MAX_MOVES - P.NbMoves()) / 2;
        int max = (int)(Position.MAX_MOVES + 1 - P.NbMoves()) / 2;
        if (weak)
        {
            min = -1;
            max = 1;
        }
        while (min < max)
        {                    // iteratively narrow the min-max exploration window
            int med = min + (max - min) / 2;
            if (med <= 0 && min / 2 < med) med = min / 2;
            else if (med >= 0 && max / 2 > med) med = max / 2;
            int r = Negamax(ref P, med, med + 1);   // use a null depth window to know if the actual score is greater or smaller than med
            if (r <= med) max = r;
            else min = r;
        }
        return min;
    }

    /// <summary>
    /// Gets the number of explored nodes in the current search
    /// </summary>
    /// <returns>The nodeCount</returns>
    public ulong GetNodeCount()
    {
        return nodeCount;
    }

    public void Reset()
    {
        nodeCount = 0;
        transTable.Reset();
    }
}

