﻿using SpriteMaster.Types;

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Extensions;

internal static class NumericsExt {
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static long KiB(this long value) => value * 1024L;
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static long MiB(this long value) => value.KiB() * 1024L;
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static long GiB(this long value) => value.MiB() * 1024L;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static ulong KiB(this ulong value) => value * 1024UL;
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static ulong MiB(this ulong value) => value.KiB() * 1024L;
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static ulong GiB(this ulong value) => value.MiB() * 1024L;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static long KiB(this int value) => value.Widen().KiB();
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static long MiB(this int value) => value.Widen().MiB();
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static long GiB(this int value) => value.Widen().GiB();

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static ulong KiB(this uint value) => value.Widen().KiB();
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static ulong MiB(this uint value) => value.Widen().MiB();
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static ulong GiB(this uint value) => value.Widen().GiB();

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static int CountLeadingZeros(this byte value) => value.Widen().CountLeadingZeros() - 8;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static int CountLeadingZeros(this sbyte value) => value.Widen().CountLeadingZeros() - 8;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static int CountLeadingZeros(this ushort value) => value.Widen().CountLeadingZeros() - 16;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static int CountLeadingZeros(this short value) => value.Widen().CountLeadingZeros() - 16;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static int CountLeadingZeros(this uint value) => BitOperations.LeadingZeroCount(value);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static int CountLeadingZeros(this int value) => value.Unsigned().CountLeadingZeros();

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static int CountLeadingZeros(this ulong value) => BitOperations.LeadingZeroCount(value);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static int CountLeadingZeros(this long value) => value.Unsigned().CountLeadingZeros();

    // Extracts a byte (8 bits) worth of data from a provided value, from the given offset
    // Example: ExtractByte(0x00F0, 8) would return 0xF
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static byte ExtractByte(this byte value, int offset) {
        offset.AssertZero();
        return value;
    }

    // Extracts a byte (8 bits) worth of data from a provided value, from the given offset
    // Example: ExtractByte(0x00F0, 8) would return 0xF
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static byte ExtractByte(this ushort value, int offset) {
        Math.Abs(offset).AssertLess(sizeof(ushort) * 8);
        return (byte)((value >> offset) & 0xFFU);
    }

    // Extracts a byte (8 bits) worth of data from a provided value, from the given offset
    // Example: ExtractByte(0x00F0, 8) would return 0xF
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static byte ExtractByte(this uint value, int offset) {
        Math.Abs(offset).AssertLess(sizeof(uint) * 8);
        return (byte)((value >> offset) & 0xFFU);
    }

    // Extracts a byte (8 bits) worth of data from a provided value, from the given offset
    // Example: ExtractByte(0x00F0, 8) would return 0xF
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static byte ExtractByte(this ulong value, int offset) {
        Math.Abs(offset).AssertLess(sizeof(ulong) * 8);
        return (byte)((value >> offset) & 0xFFU);
    }

    // Extracts a byte (8 bits) worth of data from a provided value, from the given offset
    // Example: ExtractByte(0x00F0, 8) would return 0xF
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static byte ExtractByte(this sbyte value, int offset) => ExtractByte((byte)value, offset);

    // Extracts a byte (8 bits) worth of data from a provided value, from the given offset
    // Example: ExtractByte(0x00F0, 8) would return 0xF
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static byte ExtractByte(this short value, int offset) => ExtractByte((ushort)value, offset);

    // Extracts a byte (8 bits) worth of data from a provided value, from the given offset
    // Example: ExtractByte(0x00F0, 8) would return 0xF
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static byte ExtractByte(this int value, int offset) => ExtractByte((uint)value, offset);

    // Extracts a byte (8 bits) worth of data from a provided value, from the given offset
    // Example: ExtractByte(0x00F0, 8) would return 0xF
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static byte ExtractByte(this long value, int offset) => ExtractByte((ulong)value, offset);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this long number) => number.ToString("G");

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this int number) => number.ToString("G");

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this short number) => number.ToString("G");

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this sbyte number) => number.ToString("G");

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this ulong number) => number.ToString("G");

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this uint number) => number.ToString("G");

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this ushort number) => number.ToString("G");

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this byte number) => number.ToString("G");

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this long number, string delimiter = ",", uint delimitCount = 3) {
        return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this int number, string delimiter = ",", uint delimitCount = 3) {
        return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this short number, string delimiter = ",", uint delimitCount = 3) {
        return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this sbyte number, string delimiter = ",", uint delimitCount = 3) {
        return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this ulong number, string delimiter = ",", uint delimitCount = 3) {
        return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this uint number, string delimiter = ",", uint delimitCount = 3) {
        return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this ushort number, string delimiter = ",", uint delimitCount = 3) {
        return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string Delimit(this byte number, string delimiter = ",", uint delimitCount = 3) {
        return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
    }

    private static string Delimit(this string valueString, string delimiter, uint delimitCount) {
        delimitCount.AssertPositive();
        delimiter.IsNormalized().AssertTrue();

        delimiter = delimiter.Reversed();

        string result = "";
        char[] reversedString = valueString.ToCharArray().Reverse();
        for (int i = 0; i < reversedString.Length; ++i) {
            if (i != 0 && Char.IsNumber(reversedString[i]) && (i % delimitCount) == 0) {
                result += delimiter;
            }
            result += reversedString[i];
        }

        return result.Reverse().Normalize();
    }

    internal enum DataFormat {
        IEC,
        JEDEC,
        Metric
    }

    private static readonly string[] DecimalSuffixTable = {
        "B",
        "KB",
        "MB",
        "GB",
        "TB",
        "PB",
        "EB",
        "ZB",
        "YB",
        "HB"
    };

    private static readonly string[] BinarySuffixTable = {
        "B",
        "KiB",
        "MiB",
        "GiB",
        "TiB",
        "PiB",
        "EiB",
        "ZiB",
        "YiB",
        "HiB"
    };

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string AsDataSize(this long value, DataFormat format = DataFormat.IEC, int decimals = 2) {
        value.AssertNotNegative();
        return AsDataSize((ulong)value, format, decimals);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string AsDataSize(this int value, DataFormat format = DataFormat.IEC, int decimals = 2) {
        value.AssertNotNegative();
        return AsDataSize((ulong)value, format, decimals);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string AsDataSize(this short value, DataFormat format = DataFormat.IEC, int decimals = 2) {
        value.AssertNotNegative();
        return AsDataSize((ulong)value, format, decimals);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string AsDataSize(this sbyte value, DataFormat format = DataFormat.IEC, int decimals = 2) {
        value.AssertNotNegative();
        return AsDataSize((ulong)value, format, decimals);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string AsDataSize(this uint value, DataFormat format = DataFormat.IEC, int decimals = 2) {
        return AsDataSize((ulong)value, format, decimals);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string AsDataSize(this ushort value, DataFormat format = DataFormat.IEC, int decimals = 2) {
        return AsDataSize((ulong)value, format, decimals);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string AsDataSize(this byte value, DataFormat format = DataFormat.IEC, int decimals = 2) {
        return AsDataSize((ulong)value, format, decimals);
    }

    internal static string AsDataSize(this ulong number, DataFormat format = DataFormat.IEC, int decimals = 2) {
        decimals.AssertNotNegative();
        uint fraction = (format == DataFormat.Metric) ? 1000U : 1024U;

        var suffixTable = (format == DataFormat.IEC) ? BinarySuffixTable : DecimalSuffixTable;

        // Maintain fraction?
        double value = (double)number;
        // TODO : This can be done in constant time, but meh.
        int suffixIndex = 0;
        while (value >= fraction && suffixIndex < suffixTable.Length) {
            value /= fraction;
            ++suffixIndex;
        }

        return string.Format($"{{0:0.00}} {suffixTable[suffixIndex]}", value);
    }

#if NET6_0_OR_GREATER
    // https://graphics.stanford.edu/~seander/bithacks.html#DetermineIfPowerOf2
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static bool IsPow2(this ulong value) => System.Numerics.BitOperations.IsPow2(value);
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static bool IsPow2(this long value) => System.Numerics.BitOperations.IsPow2(value);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static bool IsPow2(this uint value) => System.Numerics.BitOperations.IsPow2(value);
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static bool IsPow2(this int value) => System.Numerics.BitOperations.IsPow2(value);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static bool IsPow2(this ushort value) => value.Widen().IsPow2();
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static bool IsPow2(this short value) => value.Widen().IsPow2();

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static bool IsPow2(this byte value) => value.Widen().IsPow2();
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static bool IsPow2(this sbyte value) => value.Widen().IsPow2();
#else
	// https://graphics.stanford.edu/~seander/bithacks.html#DetermineIfPowerOf2
	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static bool IsPow2(this ulong value) => (value != 0UL) && (0UL == (value & (value - 1UL)));
	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static bool IsPow2(this long value) => value >= 0L && value.Unsigned().IsPow2();

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static bool IsPow2(this uint value) => (value != 0U) && (0U == (value & (value - 1U)));
	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static bool IsPow2(this int value) => value >= 0 && value.Unsigned().IsPow2();

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static bool IsPow2(this ushort value) => value.Widen().IsPow2();
	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static bool IsPow2(this short value) => value.Widen().IsPow2();

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static bool IsPow2(this byte value) => value.Widen().IsPow2();
	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static bool IsPow2(this sbyte value) => value.Widen().IsPow2();
#endif
}
