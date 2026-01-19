using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace c04s.src;

/**
 * A class to solve Connect 4 position using Nagemax variant of min-max algorithm.
 */
internal class Solver
{
    private ulong nodeCount; // counter of explored nodes.

    private int[] columnOrder;

    public Solver()
    {
        nodeCount = 0;
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
    public int Negamax(Position P, int alpha, int beta) {
        Trace.Assert(alpha < beta);
        nodeCount++; // increment counter of explored nodes

        if (P.NbMoves() == Position.WIDTH * Position.HEIGHT) // check for draw game
        {
            return 0;
        }

        for (int x = 0; x < Position.WIDTH; x++) // check if current player can win next move
        {
            if (P.CanPlay(x) && P.IsWinningMove(x))
            {
                return (Position.WIDTH * Position.HEIGHT + 1 - (int) P.NbMoves())/2;
            }
        }

        int max = (Position.WIDTH * Position.HEIGHT - 1 - (int) P.NbMoves()) / 2;   // upper bound of our score as we cannot win immediately
        if (beta > max)
        {
            beta = max;                     // there is no need to keep beta above our max possible score.
            if (alpha >= beta)  // prune the exploration if the [alpha;beta] window is empty.
            {
                return beta;
            }
        }

        for (int x = 0; x < Position.WIDTH; x++) // compute the score of all possible next move and keep the best one
        {
            if (P.CanPlay(columnOrder[x]))
            {
                Position P2 = (Position)P.Clone();
                P2.Play(columnOrder[x]);               // It's opponent turn in P2 position after current player plays x column.
                int score = -Negamax(P2, -beta, -alpha); // explore opponent's score within [-beta;-alpha] windows:
                                                         // no need to have good precision for score better than beta (opponent's score worse than -beta)
                                                         // no need to check for score worse than alpha (opponent's score worse better than -alpha)
                if (score >= beta) // prune the exploration if we find a possible move better than what we were looking for.
                {
                    return score;
                }
                if (score > alpha) // reduce the [alpha;beta] window for next exploration, as we only 
                {                  // need to search for a position that is better than the best so far.
                    alpha = score;
                }
            }
        }
        return alpha;
    }

    /// <summary>
    /// Initialize the stack for Negamax
    /// </summary>
    /// <param name="P"></param>
    /// <returns>The final score of all posible nodes</returns>
    public int Solve(Position P, bool weak = false)
    {
        nodeCount = 0;
        if (weak)
        {
            return Negamax(P, -1, 1);
        }
        else
        {
            return Negamax(P, -Position.WIDTH * Position.HEIGHT / 2, Position.WIDTH * Position.HEIGHT / 2);
        }
    }

    /// <summary>
    /// Gets the number of explored nodes in the current search
    /// </summary>
    /// <returns>The nodeCount</returns>
    public ulong GetNodeCount()
    {
        return nodeCount;
    }
}

