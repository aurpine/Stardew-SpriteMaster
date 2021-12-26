﻿using SpriteMaster.Extensions;
using System;
using System.Runtime.CompilerServices;

namespace SpriteMaster;
sealed class TexelTimer {
	private double DurationPerTexel = 0.0;
	private const int MaxDurationCounts = 50;

	[MethodImpl(Runtime.MethodImpl.Hot)]
	internal void Add(int texels, in TimeSpan duration) {
		// Avoid a division by zero
		if (texels == 0) {
			return;
		}

		var texelDuration = (double)duration.Ticks / texels;
		DurationPerTexel -= DurationPerTexel / MaxDurationCounts;
		DurationPerTexel += texelDuration / MaxDurationCounts;
	}

	[MethodImpl(Runtime.MethodImpl.Hot)]
	internal void Add(TextureAction action, in TimeSpan duration) => Add(action.Texels, duration);

	[MethodImpl(Runtime.MethodImpl.Hot)]
	internal TimeSpan Estimate(int texels) => TimeSpan.FromTicks((DurationPerTexel * texels).NextLong());

	[MethodImpl(Runtime.MethodImpl.Hot)]
	internal TimeSpan Estimate(TextureAction action) => Estimate(action.Texels);
}
