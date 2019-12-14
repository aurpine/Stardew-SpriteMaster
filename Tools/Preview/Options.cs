﻿namespace SpriteMaster.Tools.Preview;

internal sealed class Options {
	internal bool Preview { get; init; } = false;
	internal List<string> Paths { get; init; } = new();
}
