﻿using SpriteMaster.Configuration;
using SpriteMaster.Types;

using System;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Extensions;

internal static class Common {
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static void ConditionalSet<T>(this ref T obj, bool conditional, in T value) where T : struct {
        if (conditional) {
            obj = value;
        }
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static void ConditionalSet<T>(this ref T obj, in T? value) where T : struct {
        if (value.HasValue) {
            obj = value.Value;
        }
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static WeakReference<T> MakeWeak<T>(this T obj) where T : class => new(obj);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static int ClampDimension(this int value) => Math.Min(value, Config.ClampDimension);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static Vector2I ClampDimension(this Vector2I value) => value.Min(Config.ClampDimension);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static void Swap<T>(ref T l, ref T r) => (r, l) = (l, r);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static string GetTypeName(this object obj) => obj.GetType().Name;
}
