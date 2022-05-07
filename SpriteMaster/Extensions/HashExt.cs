﻿using System.Text;

namespace SpriteMaster.Extensions;

internal static class HashExt {
	internal static int GetSafeHash(this string? value) => (int)Encoding.Unicode.GetBytes(value ?? "").Hash();
	//internal static int GetSafeHash(this char[] value) => (int)Hashing.Hash(Encoding.Unicode.GetBytes(value ?? Array.Empty<char>()));
	internal static int GetSafeHash(this StringBuilder value) => value.ToString().GetSafeHash();

	internal static int GetSafeHash(this byte value) => value.GetHashCode();
	internal static int GetSafeHash(this sbyte value) => value.GetHashCode();
	internal static int GetSafeHash(this ushort value) => value.GetHashCode();
	internal static int GetSafeHash(this short value) => value.GetHashCode();
	internal static int GetSafeHash(this uint value) => value.GetHashCode();
	internal static int GetSafeHash(this int value) => value.GetHashCode();
	internal static int GetSafeHash(this ulong value) => value.GetHashCode();
	internal static int GetSafeHash(this long value) => value.GetHashCode();

	internal static int GetSafeHash<T>(this T? value) => value switch {
		null => Hashing.Null32,
		string s => s.GetSafeHash(),
		StringBuilder s => s.ToString().GetSafeHash(),
		_ => value.GetHashCode()
	};
}
