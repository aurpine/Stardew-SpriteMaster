﻿using SpriteMaster.Types;
using System;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Resample;

static class TextureEncode {
	[MethodImpl(Runtime.MethodImpl.Hot)]
	internal static unsafe Span<byte> Encode(ReadOnlySpan<byte> data, ref TextureFormat format, Vector2I dimensions, bool hasAlpha, bool isPunchthroughAlpha, bool isMasky, bool hasR, bool hasG, bool hasB) =>
		Encoder.BCnBlockEncoder.Encode(data, ref format, dimensions, hasAlpha, isPunchthroughAlpha, isMasky, hasR, hasG, hasB);
}
