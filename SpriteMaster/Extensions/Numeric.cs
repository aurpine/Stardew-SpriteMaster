﻿using System;

namespace SpriteMaster.Extensions {
	internal static class Numeric {
		// Extracts a byte (8 bits) worth of data from a provided value, from the given offset
		// Example: ExtractByte(0x00F0, 8) would return 0xF
		internal static byte ExtractByte (this byte value, int offset) {
			Contract.AssertZero(offset);
			return value;
		}

		// Extracts a byte (8 bits) worth of data from a provided value, from the given offset
		// Example: ExtractByte(0x00F0, 8) would return 0xF
		internal static byte ExtractByte (this ushort value, int offset) {
			Contract.AssertLess(Math.Abs(offset), sizeof(ushort) * 8);
			return unchecked((byte)((value >> offset) & 0xFFU));
		}

		// Extracts a byte (8 bits) worth of data from a provided value, from the given offset
		// Example: ExtractByte(0x00F0, 8) would return 0xF
		internal static byte ExtractByte (this uint value, int offset) {
			Contract.AssertLess(Math.Abs(offset), sizeof(uint) * 8);
			return unchecked((byte)((value >> offset) & 0xFFU));
		}

		// Extracts a byte (8 bits) worth of data from a provided value, from the given offset
		// Example: ExtractByte(0x00F0, 8) would return 0xF
		internal static byte ExtractByte (this ulong value, int offset) {
			Contract.AssertLess(Math.Abs(offset), sizeof(ulong) * 8);
			return unchecked((byte)((value >> offset) & 0xFFU));
		}

		// Extracts a byte (8 bits) worth of data from a provided value, from the given offset
		// Example: ExtractByte(0x00F0, 8) would return 0xF
		internal static byte ExtractByte (this sbyte value, int offset) {
			return ExtractByte(unchecked((byte)value), offset);
		}

		// Extracts a byte (8 bits) worth of data from a provided value, from the given offset
		// Example: ExtractByte(0x00F0, 8) would return 0xF
		internal static byte ExtractByte (this short value, int offset) {
			return ExtractByte(unchecked((ushort)value), offset);
		}

		// Extracts a byte (8 bits) worth of data from a provided value, from the given offset
		// Example: ExtractByte(0x00F0, 8) would return 0xF
		internal static byte ExtractByte (this int value, int offset) {
			return ExtractByte(unchecked((uint)value), offset);
		}

		// Extracts a byte (8 bits) worth of data from a provided value, from the given offset
		// Example: ExtractByte(0x00F0, 8) would return 0xF
		internal static byte ExtractByte (this long value, int offset) {
			return ExtractByte(unchecked((ulong)value), offset);
		}

		internal static string Delimit (this long number) {
			return number.ToString("G");
		}

		internal static string Delimit (this int number) {
			return number.ToString("G");
		}

		internal static string Delimit (this short number) {
			return number.ToString("G");
		}

		internal static string Delimit (this sbyte number) {
			return number.ToString("G");
		}

		internal static string Delimit (this ulong number) {
			return number.ToString("G");
		}

		internal static string Delimit (this uint number) {
			return number.ToString("G");
		}

		internal static string Delimit (this ushort number) {
			return number.ToString("G");
		}

		internal static string Delimit (this byte number) {
			return number.ToString("G");
		}

		internal static string Delimit (this long number, string delimiter = ",", uint delimitCount = 3) {
			return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
		}

		internal static string Delimit (this int number, string delimiter = ",", uint delimitCount = 3) {
			return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
		}

		internal static string Delimit (this short number, string delimiter = ",", uint delimitCount = 3) {
			return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
		}

		internal static string Delimit (this sbyte number, string delimiter = ",", uint delimitCount = 3) {
			return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
		}

		internal static string Delimit (this ulong number, string delimiter = ",", uint delimitCount = 3) {
			return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
		}

		internal static string Delimit (this uint number, string delimiter = ",", uint delimitCount = 3) {
			return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
		}

		internal static string Delimit (this ushort number, string delimiter = ",", uint delimitCount = 3) {
			return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
		}

		internal static string Delimit (this byte number, string delimiter = ",", uint delimitCount = 3) {
			return Delimit(number.ToString(), delimiter.Normalize(), delimitCount);
		}

		private static string Delimit (this string valueString, string delimiter, uint delimitCount) {
			Contract.AssertPositive(delimitCount);
			Contract.AssertTrue(delimiter.IsNormalized());

			delimiter = delimiter.Reversed();

			string result = "";
			char[] reversedString = valueString.ToCharArray().Reverse();
			foreach (int i in 0.Until(reversedString.Length)) {
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

		private static readonly string[] DecimalSuffixTable =
		{
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

		private static readonly string[] BinarySuffixTable =
		{
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

		internal static string AsDataSize (this long value, DataFormat format = DataFormat.IEC, int decimals = 2) {
			Contract.AssertNotNegative(value);
			return AsDataSize((ulong)value, format, decimals);
		}

		internal static string AsDataSize (this int value, DataFormat format = DataFormat.IEC, int decimals = 2) {
			Contract.AssertNotNegative(value);
			return AsDataSize((ulong)value, format, decimals);
		}

		internal static string AsDataSize (this short value, DataFormat format = DataFormat.IEC, int decimals = 2) {
			Contract.AssertNotNegative(value);
			return AsDataSize((ulong)value, format, decimals);
		}

		internal static string AsDataSize (this sbyte value, DataFormat format = DataFormat.IEC, int decimals = 2) {
			Contract.AssertNotNegative(value);
			return AsDataSize((ulong)value, format, decimals);
		}

		internal static string AsDataSize (this uint value, DataFormat format = DataFormat.IEC, int decimals = 2) {
			return AsDataSize((ulong)value, format, decimals);
		}

		internal static string AsDataSize (this ushort value, DataFormat format = DataFormat.IEC, int decimals = 2) {
			return AsDataSize((ulong)value, format, decimals);
		}

		internal static string AsDataSize (this byte value, DataFormat format = DataFormat.IEC, int decimals = 2) {
			return AsDataSize((ulong)value, format, decimals);
		}

		internal static string AsDataSize (this ulong number, DataFormat format = DataFormat.IEC, int decimals = 2) {
			Contract.AssertNotNegative(decimals);
			uint fraction = (format == DataFormat.Metric) ? 1000U : 1024U;

			var SuffixTable = (format == DataFormat.IEC) ? BinarySuffixTable : DecimalSuffixTable;

			// Maintain fraction?
			double value = (double)number;
			// TODO : This can be done in constant time, but meh.
			int suffixIndex = 0;
			while (value >= fraction && suffixIndex < SuffixTable.Length) {
				value /= fraction;
				++suffixIndex;
			}

			return string.Format("{0:0.00}", value) + $" {SuffixTable[suffixIndex]}";
		}
	}
}
