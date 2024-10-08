﻿using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Extensions;

internal static class ThreadingExt {
    [ThreadStatic]
    // ReSharper disable once ThreadStaticFieldHasInitializer : will be false, correctly, on non-UI threads
    internal static readonly bool IsMainThread = true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EnsureMainThread() {
        if (!IsMainThread) {
            ThrowHelper.ThrowInvalidOperationException("Operation not called on UI thread");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ExecuteOnMainThread(Action action) {
        if (!IsMainThread) {
            Threading.BlockOnUIThread(action);
        }

        action();
    }
}
