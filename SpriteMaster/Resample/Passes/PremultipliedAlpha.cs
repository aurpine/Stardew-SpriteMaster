﻿using SpriteMaster.Types;
using System;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Resample.Passes;

internal static partial class PremultipliedAlpha {
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static void Apply(Span<Color16> data, Vector2I size, bool full) {
        ApplyScalar(data, size, full);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static void Reverse(Span<Color16> data, Vector2I size, bool full) {
        ReverseScalar(data, size, full);
    }
}
