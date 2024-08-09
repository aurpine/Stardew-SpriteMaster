using SpriteMaster.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteMaster.Resample.Scalers.Identity;

internal sealed partial class Scaler {
	internal sealed class ScalerInterface : IScaler {
		internal static readonly ScalerInterface Instance = new();

		public IScalerInfo Info => ScalerInfo.Instance;

		uint IScaler.MinScale => Scaler.MinScale;
		uint IScaler.MaxScale => Scaler.MaxScale;
		uint IScaler.ClampScale(uint scale) => 1;

		public Span<Color16> Apply(
			Resample.Scalers.Config configuration,
			uint scaleMultiplier,
			ReadOnlySpan<Color16> sourceData,
			Vector2I sourceSize,
			Span<Color16> targetData,
			Vector2I targetSize) =>
				Scaler.Apply((Config)configuration, scaleMultiplier, sourceData, sourceSize, targetData, targetSize);

		public Resample.Scalers.Config CreateConfig(Vector2B wrapped, bool hasAlpha, bool gammaCorrected) => new Config(
			wrapped,
			hasAlpha,
			gammaCorrected);
	}
}
