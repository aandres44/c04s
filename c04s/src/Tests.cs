using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace c04s.src;

public class Tests
{

    /**
     * Here is our benchmark data set of 6000 positions with their expected scores:
     * Test Set (1000 test cases each)	Test Set name	nb moves	        nb remaining moves
     * Test_L3_R1	                    End-Easy	    28 < moves	        remaining < 14
     * Test_L2_R1	                    Middle-Easy	    14 < moves <= 28	remaining < 14
     * Test_L2_R2	                    Middle-Medium	14 < moves <= 28	14 <= remaining < 28
     * Test_L1_R1	                    Begin-Easy	    moves <= 14	        remaining < 14
     * Test_L1_R2	                    Begin-Medium	moves <= 14	        14 <= remaining < 28
     * Test_L1_R3	                    Begin-Hard	    moves <= 14	        28 <= remaining
     */

    /**
     * Test position 44455554221:
     * .......
     * .......
     * ...ox..
     * ...xo..
     * .o.ox..
     * xx.xo..
     */

    /**
    * Reads Connect 4 positions, line by line, from standard input
    * and writes one line per position to standard output containing:
    *  - score of the position
    *  - number of nodes explored
    *  - time spent in microsecond to solve the position.
    *
    *  Any invalid position (invalid sequence of move, or already won game)
    *  will generate an error message to standard error and an empty line to standard output.
    */
    public static void RunTests()
    {
        Solver solver = new();
        string line;
        int l = 1; // Initialize the line counter
        Stopwatch swt = Stopwatch.StartNew();

        // Read lines until the end of the stream
        while ((line = Console.ReadLine()!) != null)
        {
            line = line.Split(' ').First();
            Position P = new();
            if (P.Play(line) != line.Length)
            {
                Trace.WriteLine($"Line {l}: Invalid move {P.NbMoves() + 1} \"{line}\"\n");
            }
            else
            {
                Stopwatch sw = Stopwatch.StartNew();
                int score = solver.Solve(P);
                sw.Stop();
                Console.WriteLine($"{line} {score} {solver.GetNodeCount()} {sw.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000L)}");
            }
            ConsoleGame.DrawBoard(P.GetCurrentPlayerPositions(), P.GetOtherPlayerPositions());
            l++; // Increment the line counter
        }
        swt.Stop();
        Console.WriteLine($"Total {swt.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000L)}");
    }

    /**
    * Reads Connect 4 position, one line, from standard input
    * and writes one line to standard output containing:
    *  - score of the position
    *  - number of nodes explored
    *  - time spent in microsecond to solve the position.
    *
    *  Any invalid position (invalid sequence of move, or already won game) 
    *  will generate an error message to standard error and an empty line to standard output.
    */
    public static void TestLine()
    {
        Solver solver = new();
        string line = Console.ReadLine() ?? "";
        line = line.Split(' ').First();
        Position P = new();
        if (P.Play(line) != line.Length)
        {
            Trace.WriteLine($"Invalid move {P.NbMoves() + 1} \"{line}\"\n");
        }
        else
        {
            Stopwatch sw = Stopwatch.StartNew();
            int score = solver.Solve(P);
            sw.Stop();
            Console.WriteLine($"{line} Score: {score} Nodes: {solver.GetNodeCount()} Time: {sw.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000L)}");
        }
        ConsoleGame.DrawBoard(P.GetCurrentPlayerPositions(), P.GetOtherPlayerPositions());
    }

    /**
    * Reads Connect 4 positions, line by line, from a file
    * and writes one line per position to standard output containing:
    *  - score of the position
    *  - number of nodes explored
    *  - time spent in microsecond to solve the position.
    *
    *  Any invalid position (invalid sequence of move, or already won game) 
    *  will generate an error message to standard error and an empty line to standard output.
    */
    public static void RunTestsFromFile()
    {
        Solver solver = new();
        string line;
        string filePath = "Data/Test_L3_R1";
        // Use a StreamReader with the standard input stream
        using StreamReader reader = new(filePath);
        int l = 1; // Initialize the line counter
        Stopwatch swt = Stopwatch.StartNew();
        // Read lines until the end of the stream
        while ((line = reader.ReadLine()!) != null)
        {
            line = line.Split(' ').First();
            Position P = new();
            if (P.Play(line) != line.Length)
            {
                Trace.WriteLine($"Line {l}: Invalid move {P.NbMoves() + 1} \"{line}\"\n");
            }
            else
            {
                Stopwatch sw = Stopwatch.StartNew();
                int score = solver.Solve(P);
                sw.Stop();
                Console.WriteLine($"{line} {score} {solver.GetNodeCount()} {sw.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000L)}");
            }
            Console.WriteLine("\n");
            l++; // Increment the line counter
        }
        swt.Stop();
        Console.WriteLine($"Total {swt.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000L)}");
    }
}

