﻿using SpriteMaster.Types;
using System;

namespace SpriteMaster.Resample.Scalers.EPX;

internal sealed partial class Scaler {
	internal sealed class ScalerInterface : IScaler {
		internal static readonly ScalerInterface Instance = new(false);
		internal static readonly ScalerInterface InstanceLegacy = new(true);

		public IScalerInfo Info => Legacy ? ScalerInfo.InstanceLegacy : ScalerInfo.Instance;

		uint IScaler.MinScale => Scaler.MinScale;

		uint IScaler.MaxScale => Scaler.MaxScale;

		uint IScaler.ClampScale(uint scale) => Scaler.ClampScale(scale);

		private readonly bool Legacy;

		public Span<Color16> Apply(
			Resample.Scalers.Config configuration,
			uint scaleMultiplier,
			ReadOnlySpan<Color16> sourceData,
			Vector2I sourceSize,
			Span<Color16> targetData,
			Vector2I targetSize
		) =>
			Scaler.Apply((Config)configuration, scaleMultiplier, sourceData, sourceSize, targetData, targetSize);

		public Resample.Scalers.Config CreateConfig(Vector2B wrapped, bool hasAlpha, bool gammaCorrected, int saturation, int lightness) => new Config(
			wrapped: wrapped,
			hasAlpha: hasAlpha,
			luminanceWeight: SMConfig.Resample.Common.LuminanceWeight,
			gammaCorrected: gammaCorrected,
			equalColorTolerance: SMConfig.Resample.Common.EqualColorTolerance,
			useRedmean: SMConfig.Resample.UseRedmean,
			smoothCompare: !Legacy,
			saturation: saturation,
			lightness: lightness
		);

		private ScalerInterface(bool legacy) {
			Legacy = legacy;
		}
	}
}
