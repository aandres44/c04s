using c04s.src;
using System;

namespace c04s.src;

/// <summary>
/// Class used to control the game, simulation and testing of the game in Console
/// </summary>
public class ConsoleGame
{
    /// <summary>
    /// Static method that prints the provided board to the Console
    /// </summary>
    /// <param name="board"></param>
	public static void DrawBoard(int[,] board)
	{
        for (int i = board.GetLength(1) - 1; i >= 0; i--)
        {
            for(int j = 0; j < board.GetLength(0); j++)
            {
                int spot = board[j, i];
                if (spot == 1)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                }
                else if (spot == 2)
                {
                    Console.BackgroundColor = ConsoleColor.Blue;
                }
                Console.Write(spot);
                Console.BackgroundColor = ConsoleColor.Black;
            }
            Console.WriteLine();
        }
    }
}
