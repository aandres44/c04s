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
    /// <param name="cPP">currentPlayerPositions</param>
    /// <param name="oPP">otherPlayerPositions</param>
	public static void DrawBoard(ulong cPP, ulong oPP)
	{
        string cPPs = Convert.ToString((long)cPP, 2).PadLeft(63 - 14, '0');
        string oPPs = Convert.ToString((long)oPP, 2).PadLeft(63 - 14, '0');
        Console.WriteLine(cPPs);
        Console.WriteLine(oPPs);

        for (int i = 0; i < cPPs.Length; i++)
        {
            if (i % 7 == 0)
            {
                Console.WriteLine();
            }
            string spot = "0";
            if (cPPs[i] == '1')
            {
                Console.BackgroundColor = ConsoleColor.Red;
                spot = "1";
            }
            else if (oPPs[i] == '1')
            {
                Console.BackgroundColor = ConsoleColor.Blue;
                spot = "2";
            }
            Console.Write(spot);
            Console.BackgroundColor = ConsoleColor.Black;
        }
        Console.WriteLine();
    }
}
