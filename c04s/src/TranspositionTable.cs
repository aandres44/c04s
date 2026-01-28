using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace c04s.src;

/// <summary>
/// Transposition Table is a simple hash map with fixed storage size.
/// In case of collision we keep the last entry and override the previous one.
/// </summary>
public sealed class TranspositionTable
{
    private readonly ulong[] table;     // packed entries
    private readonly int size;          // prime table size
    private byte currentAge = 1;

    public TranspositionTable(int sizeMB)
    {
        long bytes = (long)sizeMB * 1024 * 1024;
        int entries = (int)(bytes / sizeof(ulong));

        size = PreviousPrime(entries);   // Step 11: prime size
        table = GC.AllocateUninitializedArray<ulong>(size);
        Array.Clear(table);              // age = 0 means empty
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Index(ulong key) => (int)(key % (ulong)size);

    /*
	* Empty the Transition Table.
	*/
    public void Reset()
    {
        // O(1) Reset: Just increment the age
        if (++currentAge == 0)
        { // fill everything with 0, because 0 value means missing data
            Array.Clear(table); // Only full clear once every 255 resets
            currentAge = 1;
        }
    }

    /**
    * Store a value for a given key
    * @param key: 56-bit key
    * @param value: non-null 8-bit value. null (0) value are used to encode missing data.
    */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Put(ulong key, byte val, sbyte bestMove)
    {
        uint key32 = (uint)(key ^ (key >> 32)); // Step 11: key truncation with mixing
        ulong packed =
            ((ulong)key32 << 32) |
            ((ulong)val << 24) |
            ((ulong)currentAge << 16) |
            ((ulong)(byte)bestMove << 8);

        table[Index(key)] = packed;
    }

    /** 
    * Get the value of a key
    * @param key
    * @return 8-bit value associated with the key if present, 0 otherwise.
    */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Get(ulong key)
    {
        ulong entry = table[Index(key)];
        if (entry == 0) return 0;

        uint storedKey = (uint)(entry >> 32);
        byte age = (byte)(entry >> 16);

        uint key32 = (uint)(key ^ (key >> 32));

        if (storedKey == key32 && age == currentAge)
            return (byte)(entry >> 24);

        return 0;
    }

    // -------- Prime helper (runs only at init) --------
    private static int PreviousPrime(int n)
    {
        while (n > 2)
        {
            if (IsPrime(n)) return n;
            n--;
        }
        return 2;
    }

    private static bool IsPrime(int n)
    {
        if ((n & 1) == 0) return n == 2;
        int r = (int)Math.Sqrt(n);
        for (int i = 3; i <= r; i += 2)
            if (n % i == 0) return false;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte GetBestMove(ulong key)
    {
        ulong entry = table[Index(key)];
        if (entry == 0) return -1;

        uint storedKey = (uint)(entry >> 32);
        byte age = (byte)(entry >> 16);
        uint key32 = (uint)(key ^ (key >> 32));

        if (storedKey == key32 && age == currentAge)
            return (sbyte)((entry >> 8) & 0xFF);

        return -1;
    }
}
