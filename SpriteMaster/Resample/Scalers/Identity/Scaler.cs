using SpriteMaster.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpriteMaster.Resample.Scalers.Identity;

internal partial class Scaler : AbstractScaler<Config, Scaler.ValueScale> {
    private const uint MinScale = 1;
    private const uint MaxScale = 1;

    internal readonly struct ValueScale : IScale {
        public readonly uint Minimum => MinScale;
        public readonly uint Maximum => MaxScale;
    }

    private static uint ClampScale(uint scale) => 1;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    private Scaler(
        Config configuration,
        uint scaleMultiplier,
        Vector2I sourceSize,
        Vector2I targetSize
    ) : base(configuration, scaleMultiplier, sourceSize, targetSize) {
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    private static Span<Color16> Apply(
        Config config,
        uint scaleMultiplier,
        ReadOnlySpan<Color16> sourceData,
        Vector2I sourceSize,
        Span<Color16> targetData,
        Vector2I targetSize) {
        Resample.Scalers.Common.ApplyValidate(config, scaleMultiplier, sourceData, sourceSize, ref targetData, targetSize);

        var scalerInstance = new Scaler(
            configuration: config,
            scaleMultiplier: scaleMultiplier,
            sourceSize: sourceSize,
            targetSize: targetSize
        );

        scalerInstance.Scale(sourceData, targetData);
        return targetData;
    }

    private void Scale(ReadOnlySpan<Color16> source, Span<Color16> destination) {
        for (var i = 0; i < source.Length; i++) {
            destination[i] = source[i];
        }
    }
}
