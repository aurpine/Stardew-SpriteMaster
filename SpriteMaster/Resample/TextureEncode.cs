﻿using SpriteMaster.Types;
using System;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Resample;

internal static class TextureEncode {
	[MethodImpl(Runtime.MethodImpl.Hot)]
	internal static Span<byte> Encode(ReadOnlySpan<Color8> data, ref TextureFormat format, Vector2I dimensions, bool hasAlpha, bool isPunchthroughAlpha, bool isMasky, bool hasR, bool hasG, bool hasB) =>
		Encoder.TeximpBlockEncoder.Encode(data, ref format, dimensions, hasAlpha, isPunchthroughAlpha, isMasky, hasR, hasG, hasB);
}
