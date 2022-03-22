﻿using SpriteMaster.Types;
using System;

namespace SpriteMaster.Resample.Scalers;

interface IScaler {
	Config CreateConfig(
		Vector2B wrapped,
		bool hasAlpha,
		bool gammaCorrected
	);

	uint MinScale { get; }
	uint MaxScale { get; }
	uint ClampScale(uint scale);

	Span<Color16> Apply(
		in Config configuration,
		uint scaleMultiplier,
		ReadOnlySpan<Color16> sourceData,
		Vector2I sourceSize,
		Span<Color16> targetData,
		Vector2I targetSize
	);

	internal static IScaler Current => new DefaultScaler.Scaler.ScalerInterface();
}