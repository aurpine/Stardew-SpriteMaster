﻿using SpriteMaster.Extensions;
using System.Runtime.InteropServices;

namespace SpriteMaster.Types;

[StructLayout(LayoutKind.Auto)]
internal readonly struct ArrayWrapper2D<T> {
    internal readonly T[] Data;
    internal readonly uint Width;
    internal readonly uint Height;
    internal readonly uint Stride;

    internal ArrayWrapper2D(T[] data, int width, int height, int stride) {
        data.AssertNotNull();
        width.AssertNotNegative();
        height.AssertNotNegative();
        stride.AssertNotNegative();

        Data = data;
        Width = width.Unsigned();
        Height = height.Unsigned();
        Stride = stride.Unsigned();
    }

    internal ArrayWrapper2D(T[] data, int width, int height) : this(data, width, height, width) { }

    private uint GetIndex(int x, int y) {
        x.AssertNotNegative();
        y.AssertNotNegative();

        return GetIndex(x.Unsigned(), y.Unsigned());
    }

    private uint GetIndex(uint x, uint y) {
        var offset = y * Stride + x;

        offset.AssertLess(Data.Length.Unsigned());

        return offset;
    }

    internal T this[int x, int y] {
        get => Data[GetIndex(x, y)];
        set => Data[GetIndex(x, y)] = value;
    }

    internal T this[uint x, uint y] {
        get => Data[GetIndex(x, y)];
        set => Data[GetIndex(x, y)] = value;
    }
}
