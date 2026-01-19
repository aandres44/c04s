using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace c04s.src;

/**
 * A class to solve Connect 4 position using Nagemax variant of min-max algorithm.
 */
internal class Solver
{
    private ulong nodeCount; // counter of explored nodes.

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
    public int Negamax(Position P) {
        nodeCount++; // increment counter of explored nodes

        if (P.NbMoves() == Position.WIDTH * Position.HEIGHT) // check for draw game
        {
            return 0;
        }


        for (int x = 0; x < Position.WIDTH; x++) // check if current player can win next move
        {
            if (P.CanPlay(x) && P.IsWinningMove(x))
            {
                return (Position.WIDTH * Position.HEIGHT+1 - (int) P.NbMoves())/2;
            }
        }
        
        int bestScore = -Position.WIDTH * Position.HEIGHT; // init the best possible score with a lower bound of score.
        for (int x = 0; x < Position.WIDTH; x++) // compute the score of all possible next move and keep the best one
        {
            if (P.CanPlay(x))
            {
                Position P2 = (Position)P.Clone();
                P2.Play(x);               // It's opponent turn in P2 position after current player plays x column.
                int score = -Negamax(P2); // If current player plays col x, his score will be the opposite of opponent's score after playing col x
                if (score > bestScore) { bestScore = score; } // keep track of best possible score so far.
            }
        }
        return bestScore;
    }

    /// <summary>
    /// Initialize the stack for Negamax
    /// </summary>
    /// <param name="P"></param>
    /// <returns>The final score of all posible nodes</returns>
    public int Solve(Position P)
    {
        nodeCount = 0;
        return Negamax(P);
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

