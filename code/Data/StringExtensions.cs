using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VoxelWorld.Data;

public static class StringExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int PowerOf31(int power)
    {
        int result = 1;
        for(int i = 0; i < power; ++i)
            result *= 31;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetPowerOf31(int power)
    {
        if(power < _pows31.Length)
            return _pows31[power];
        return PowerOf31(power);
    }
    private readonly static int[] _pows31 = Enumerable.Range(0, 64).Select(PowerOf31).ToArray();

    public static int GetConsistentHashCode(this string value)
    {
        int result = 0;
        var length = value.Length;
        for(int i = 0; i < length; ++i)
            result += value[0] * GetPowerOf31(length - i - 1);
        return result;
    }
}
