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
public class TranspositionTable
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry
    {
        public ulong Key; // 56 bits used
        public byte Val;  // 8 bits
        public byte Age;  // 8 bits
        public sbyte BestMove; // 0–6 valid, 255 = unknown
    }

    private readonly Entry[] T;
    private readonly ulong mask;
    private byte currentAge = 1;

    public TranspositionTable(int sizeMB)
	{
        int entrySize = Unsafe.SizeOf<Entry>(); // Usually 12 or 16 depending on JIT padding
        long bytes = (long)sizeMB * 1024 * 1024;
        int numEntries = (int)(bytes / entrySize);

        // Power of 2 rounding
        int pow2 = 1 << (31 - BitOperations.LeadingZeroCount((uint)numEntries));

        // GC.AllocateUninitializedArray is the fastest way to get a large array in 2026
        // because it skips the initial zero-clearing (which you handle via Age logic).
        T = GC.AllocateUninitializedArray<Entry>(pow2);
        mask = (ulong)(pow2 - 1);

        // Manually clear once at the very start to ensure Age 0 is everywhere
        Array.Clear(T, 0, T.Length);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint GetIndex(ulong key) => (uint)(key & mask);

    /*
	* Empty the Transition Table.
	*/
    public void Reset()
    {
        // O(1) Reset: Just increment the age
        if (++currentAge == 0)
        { // fill everything with 0, because 0 value means missing data
            Array.Clear(T, 0, T.Length); // Only full clear once every 255 resets
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
        int i = (int)GetIndex(key);
        ref Entry target = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(T), i);
        target.Key = key;
        target.Val = val;
        target.Age = currentAge;
        target.BestMove = bestMove;
    }

    /** 
    * Get the value of a key
    * @param key
    * @return 8-bit value associated with the key if present, 0 otherwise.
    */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Get(ulong key)
    {
        int i = (int)GetIndex(key);
        ref Entry entry = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(T), i);
        // Comparison is fast because 'Age' and 'Key' are in the same cache line
        if (entry.Key == key && entry.Age == currentAge)
        {
            return entry.Val;
        }
        return 0;
    }
}
