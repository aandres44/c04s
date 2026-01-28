using System;

namespace c04s.src;

/*
* This class helps sorting the next moves
*
* You have to add moves first with their score 
* then you can get them back in decreasing score
*
* This class implement an insertion sort that is in practice very
* efficient for small number of move to sort (max is Position::WIDTH)
* and also efficient if the move are pushed in approximatively increasing 
* order which can be acheived by using a simpler column ordering heuristic.
*/
internal sealed class MoveSorter
{
    private struct Entry
    {
        public ulong Move;
        public int Score;
    }

    // Maximum number of moves = board width
    private readonly Entry[] entries = new Entry[Position.WIDTH];

    // Number of stored moves
    private int size;

    /// <summary>
    /// Add a move with its score.
    /// Must not exceed Position.WIDTH total moves.
    /// Keeps moves sorted by increasing score internally.
    /// </summary>
    public void Add(ulong move, int score)
    {
        int pos = size++;

        // Insertion sort step
        while (pos > 0 && entries[pos - 1].Score > score)
        {
            entries[pos] = entries[pos - 1];
            pos--;
        }

        entries[pos].Move = move;
        entries[pos].Score = score;
    }

    /// <summary>
    /// Get the next move with the highest score.
    /// Removes it from the container.
    /// Returns 0 if no moves remain.
    /// </summary>
    public ulong GetNext()
    {
        if (size > 0)
            return entries[--size].Move;
        else
            return 0UL;
    }

    /// <summary>
    /// Reset the container to empty.
    /// </summary>
    public void Reset()
    {
        size = 0;
    }

    /// <summary>
    /// Create an empty sorter.
    /// </summary>
    public MoveSorter()
    {
        size = 0;
    }
}

