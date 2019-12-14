﻿using LinqFasterer;
using System.Text.RegularExpressions;

namespace SpriteMaster.Tools.Preview;

internal readonly record struct Argument(string Key, string? Value = null) {
	internal readonly bool IsCommand => Key[0] is '-' or '/';
	private static readonly Regex CommandPattern = new(@"^(?:--|-|/)(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
	internal readonly string? Command => IsCommand ? CommandPattern.Match(Key).Groups.ElementAtOrDefaultF(1)?.Value : null;

	public override readonly string ToString() => Value is null ? Key : $"{Key}={Value}";
}
