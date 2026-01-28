using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

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

    private const string DATA_PATH = "Data/";
    private const string END_EASY = DATA_PATH + "Test_L3_R1";
    private const string MIDDLE_EASY = DATA_PATH + "Test_L2_R1";
    private const string MIDDLE_MEDIUM = DATA_PATH + "Test_L2_R2";
    private const string BEGIN_EASY = DATA_PATH + "Test_L1_R1";
    private const string BEGIN_MEDIUM = DATA_PATH + "Test_L1_R2";
    private const string BEGIN_HARD = DATA_PATH + "Test_L1_R3";

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
        long start = Stopwatch.GetTimestamp();
        long totalTime = 0;
        ulong totalNodes = 0;
        var output = new StringBuilder(1 << 20); // ~1 MB buffer
        // Read lines until the end of the stream
        while ((line = Console.ReadLine()!) != null)
        {
            // Fast trim at first space (avoid Split + LINQ)
            int space = line.IndexOf(' ');
            if (space >= 0)
                line = line[..space];

            Position P = new();
            if (P.Play(line) != line.Length)
            {
#if DEBUG
                Trace.WriteLine($"Line {l}: Invalid move {P.NbMoves() + 1} \"{line}\"\n");
#endif
            }
            else
            {
                solver.Reset();
                long startSmall = Stopwatch.GetTimestamp();
                int score = solver.Solve(P);
                long microsSmall = (Stopwatch.GetTimestamp() - startSmall) * 1_000_000 / Stopwatch.Frequency;
                ulong nodes = solver.GetNodeCount();
                totalTime += microsSmall;
                totalNodes += nodes;
                output.Append(line)
                .Append(' ')
                .Append(score)
                .Append(' ')
                .Append(nodes)
                .Append(' ')
                .Append(microsSmall)
                .AppendLine();
            }
            output.AppendLine();
            l++; // Increment the line counter
        }
        Console.WriteLine(output.ToString());
        long micros = (Stopwatch.GetTimestamp() - start) * 1_000_000 / Stopwatch.Frequency;
        Console.WriteLine($"Total {micros}, mean time {totalTime / l}, mean nodes {(int)totalNodes / l}");
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
        string? line;
        string filePath = END_EASY;
        // Use a StreamReader with the standard input stream
        using StreamReader reader = new(filePath);
        int l = 1; // Initialize the line counter
        Position P = new();
        var output = new StringBuilder(1 << 20); // ~1 MB buffer
        long start = Stopwatch.GetTimestamp();
        long totalTime = 0;
        ulong totalNodes = 0;
        // Read lines until the end of the stream
        while ((line = reader.ReadLine()) != null)
        {
            int space = line.IndexOf(' ');
            line = space >= 0 ? line[..space] : line;
            P.Reset();
            if (P.Play(line) != line.Length)
            {
#if DEBUG
                Trace.WriteLine($"Line {l}: Invalid move {P.NbMoves() + 1} \"{line}\"\n");
#endif
            }
            else
            {
                solver.Reset();
                long startSmall = Stopwatch.GetTimestamp();
                int score = solver.Solve(P);
                long microsSmall = (Stopwatch.GetTimestamp() - startSmall) * 1_000_000 / Stopwatch.Frequency;
                ulong nodes = solver.GetNodeCount();
                totalTime += microsSmall;
                totalNodes += nodes;
                output.Append(line)
              .Append(' ')
              .Append(score)
              .Append(' ')
              .Append(nodes)
              .Append(' ')
              .Append(microsSmall)
              .AppendLine();
            }
            output.AppendLine();
            l++; // Increment the line counter
        }
        Console.WriteLine(output.ToString());
        long micros = (Stopwatch.GetTimestamp() - start) * 1_000_000 / Stopwatch.Frequency;
        Console.WriteLine($"Total {micros}, mean time {totalTime / l}, mean nodes {(int)totalNodes / l}");
    }
}

