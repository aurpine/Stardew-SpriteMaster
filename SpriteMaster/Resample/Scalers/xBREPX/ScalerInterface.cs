﻿using SpriteMaster.Types;
using System;

namespace SpriteMaster.Resample.Scalers.xBREPX;

internal sealed partial class Scaler {
    internal sealed class ScalerInterface : IScaler {
        internal static readonly ScalerInterface Instance = new();

        public IScalerInfo Info => ScalerInfo.Instance;

        uint IScaler.MinScale => Scaler.MinScale;

        uint IScaler.MaxScale => Scaler.MaxScale;

        uint IScaler.ClampScale(uint scale) => Scaler.ClampScale(scale);

        public Span<Color16> Apply(
            Resample.Scalers.Config configuration,
            uint scaleMultiplier,
            ReadOnlySpan<Color16> sourceData,
            Vector2I sourceSize,
            Span<Color16> targetData,
            Vector2I targetSize
        ) =>
            Scaler.Apply((Config)configuration, scaleMultiplier, sourceData, sourceSize, targetData, targetSize);

        public Resample.Scalers.Config CreateConfig(Vector2B wrapped, bool hasAlpha, bool gammaCorrected) => new Config(
            wrapped: wrapped,
            hasAlpha: hasAlpha,
            luminanceWeight: SMConfig.Resample.Common.LuminanceWeight,
            gammaCorrected: gammaCorrected,
            equalColorTolerance: SMConfig.Resample.Common.EqualColorTolerance,
            useRedmean: SMConfig.Resample.UseRedmean
        );
    }
}
