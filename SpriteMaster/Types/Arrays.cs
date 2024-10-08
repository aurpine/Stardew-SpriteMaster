﻿using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Types;

internal static class Arrays {
    private static class EmptyArrayStatic<T> {
        [ImmutableObject(true)]
        internal static readonly T[] Value = Array.Empty<T>();
    }

    [Pure, MethodImpl(Runtime.MethodImpl.Inline)]
    [ImmutableObject(true)]
    internal static T[] Empty<T>() => EmptyArrayStatic<T>.Value;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static T[] Singleton<T>(T value) => new[] { value };

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static T[] Of<T>(params T[] values) => values;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static MemoryStream Stream(this byte[] data) => new(data, 0, data.Length, true, true);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    // ReSharper disable once MethodOverloadWithOptionalParameter
    internal static MemoryStream Stream(this byte[] data, int offset = 0, int length = -1, FileAccess access = FileAccess.ReadWrite) {
        if (length == -1) {
            length = data.Length - offset;
        }
        return new MemoryStream(data, offset, length, (access != FileAccess.Read), true);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static T[] Reverse<T>(this T[] array) {
        //Contract.AssertNotNull(array);
        Array.Reverse(array);
        return array;
    }
}

internal static class Arrays<T> {
    [ImmutableObject(true)]
    internal static readonly T[] Empty = Arrays.Empty<T>();

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static T[] Singleton(T value) => Arrays.Singleton(value);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static T[] Of(params T[] values) => Arrays.Of(values);
}
