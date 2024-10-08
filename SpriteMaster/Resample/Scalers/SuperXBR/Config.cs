﻿#if !SHIPPING
using SpriteMaster.Types;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Resample.Scalers.SuperXBR;

sealed class Config : Scalers.Config {
    internal const int MaxScale = 8;

    // default, minimum, maximum, optional step

    internal readonly float EdgeStrength;
    internal readonly float Weight;
    internal readonly float EdgeShape;
    internal readonly float TextureShape;
    internal readonly float AntiRinging;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal Config(
        Vector2B wrapped,
        bool hasAlpha = true,
        bool gammaCorrected = true,
        float edgeStrength = 2.0f,
        float weight = 1.0f,
        float edgeShape = 0.0f,
        float textureShape = 0.0f,
        float antiRinging = 1.0f
    ) : base(
        wrapped: wrapped,
        hasAlpha: hasAlpha,
        gammaCorrected: gammaCorrected
    ) {
        EdgeStrength = edgeStrength;
        Weight = weight;
        EdgeShape = edgeShape;
        TextureShape = textureShape;
        AntiRinging = antiRinging;
    }
}
#endif
