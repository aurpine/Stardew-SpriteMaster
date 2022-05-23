﻿using LinqFasterer;
using Pastel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Extensions;

internal static class StringExt {
	#region General
	[MethodImpl(Runtime.MethodImpl.Inline)]
	[return: NotNullIfNotNull("str")]
	internal static string? Intern(this string? str) => str is null ? null : string.Intern(str);

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static string ToString<T>(this T? obj, in DrawingColor color) => (obj?.ToString() ?? "[null]").Pastel(color);

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static bool IsEmpty(this string str) => str.Length == 0;

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static bool IsBlank([NotNullWhen(false)] this string? str) => string.IsNullOrEmpty(str);

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static bool IsWhiteBlank([NotNullWhen(false)] this string? str) => string.IsNullOrEmpty(str?.Trim());

	internal static unsafe string Reverse(this string str) {
		str.AssertNotNull();

		var strSpan = str.AsSpan().ToSpanUnsafe();
		for (int i = 0; i < strSpan.Length / 2; ++i) {
			int endIndex = (strSpan.Length - i) - 1;
			var temp = strSpan[endIndex];
			strSpan[endIndex] = strSpan[i];
			strSpan[i] = temp;
		}

		return str;
	}

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static string Reversed(this string str) {
		str.AssertNotNull();
		return new string(str).Reverse();
	}

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static string Enquote(this string str, char quote = '\'') {
		if (str.Length >= 2 && str[0] == quote && str[^1] == quote) {
			return str;
		}
		return $"{quote}{str}{quote}";
	}

	private static readonly char[] NewlineChars = { '\n', '\r' };
	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static IEnumerable<string> Lines(this string str, bool removeEmpty = false) {
		var strings = str.Split(NewlineChars);
		var validLines = removeEmpty ? strings.WhereF(l => !l.IsBlank()) : strings;
		return validLines;
	}

	#endregion General

	#region Equality

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static bool EqualsInvariantInsensitive(this string str1, string str2) => str1.Equals(str2, System.StringComparison.InvariantCultureIgnoreCase);

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static bool EqualsOrdinal(this string str1, string str2) => str1.Equals(str2, System.StringComparison.Ordinal);

	#endregion Equality

	#region Color

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static string Colorized(this string str, DrawingColor color) => str.Pastel(color);

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static string Colorized(this string str, DrawingColor foregroundColor, DrawingColor backgroundColor) => str.Pastel(foregroundColor).PastelBg(backgroundColor);

	#endregion Color
}
