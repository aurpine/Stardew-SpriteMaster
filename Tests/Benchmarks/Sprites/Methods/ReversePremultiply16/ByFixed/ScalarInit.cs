﻿using SpriteMaster.Types;
using System.Runtime.CompilerServices;

namespace Benchmarks.Sprites.Methods.ReversePremultiply16.ByFixed;
internal static class ScalarInit {
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static unsafe void Reverse(Span<Color16> data, Vector2I size) {
		ushort lowPass = Common.PremultiplicationLowPass;

		fixed (Color16* pData = data) {
			for (int i = 0; i < data.Length; ++i) {
				var item = pData[i];

				var alpha = item.A;

				switch (alpha.Value) {
					case ushort.MaxValue:
					case var _ when alpha.Value <= lowPass:
						continue;
					default:
						pData[i] = new Color16(
							item.R.ClampedDivide(alpha),
							item.G.ClampedDivide(alpha),
							item.B.ClampedDivide(alpha),
							alpha
						);

						break;
				}
			}
		}
	}
}