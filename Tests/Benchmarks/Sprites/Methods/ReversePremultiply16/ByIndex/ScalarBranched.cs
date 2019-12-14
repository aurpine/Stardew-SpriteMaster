﻿using SpriteMaster.Types;
using System.Runtime.CompilerServices;

namespace Benchmarks.Sprites.Methods.ReversePremultiply16.ByIndex;
internal static class ScalarBranched {
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void Reverse(Span<Color16> data, Vector2I size) {
		ushort lowPass = Common.PremultiplicationLowPass;

		for (int i = 0; i < data.Length; ++i) {
			var item = data[i];

			var alpha = item.A;

			if (alpha.Value is ushort.MaxValue) {
				continue;
			}

			if (alpha.Value <= lowPass) {
				continue;
			}

			data[i].SetRgb(
				item.R.ClampedDivide(alpha),
				item.G.ClampedDivide(alpha),
				item.B.ClampedDivide(alpha)
			);
		}
	}
}