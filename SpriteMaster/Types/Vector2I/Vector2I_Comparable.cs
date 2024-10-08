﻿using SpriteMaster.Extensions;
using System;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Types;

internal partial struct Vector2I :
    IComparable,
    IComparable<Vector2I>,
    IComparable<Vector2I?>,
    IComparable<(int, int)>,
    IComparable<(int, int)?>,
    IComparable<DrawingPoint>,
    IComparable<DrawingPoint?>,
    IComparable<XNA.Point>,
    IComparable<XNA.Point?>,
    IComparable<XTilePoint>,
    IComparable<XTilePoint?>,
    IComparable<DrawingSize>,
    IComparable<DrawingSize?>,
    IComparable<XTileSize>,
    IComparable<XTileSize?> {
    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly int CompareTo(Vector2I other) => Packed.CompareTo(other.Packed);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly int CompareTo(Vector2I? other) => other.HasValue ? Packed.CompareTo(other.Value.Packed) : Packed.CompareTo(null);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly int CompareTo((int, int) other) => CompareTo((Vector2I)other);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly int CompareTo((int, int)? other) => other.HasValue ? CompareTo((Vector2I)other.Value) : Packed.CompareTo(null);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly int CompareTo(DrawingPoint other) => CompareTo((Vector2I)other);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly int CompareTo(DrawingPoint? other) => other.HasValue ? CompareTo((Vector2I)other.Value) : Packed.CompareTo(null);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly int CompareTo(XNA.Point other) => CompareTo((Vector2I)other);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly int CompareTo(XNA.Point? other) => other.HasValue ? CompareTo((Vector2I)other.Value) : Packed.CompareTo(null);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly int CompareTo(XTilePoint other) => CompareTo((Vector2I)other);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly int CompareTo(XTilePoint? other) => other.HasValue ? CompareTo((Vector2I)other.Value) : Packed.CompareTo(null);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly int CompareTo(DrawingSize other) => CompareTo((Vector2I)other);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly int CompareTo(DrawingSize? other) => other.HasValue ? CompareTo((Vector2I)other.Value) : Packed.CompareTo(null);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly int CompareTo(XTileSize other) => CompareTo((Vector2I)other);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly int CompareTo(XTileSize? other) => other.HasValue ? CompareTo((Vector2I)other.Value) : Packed.CompareTo(null);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    readonly int IComparable.CompareTo(object? other) => other switch {
        Vector2I vec => CompareTo(vec),
        DrawingPoint vec => CompareTo((Vector2I)vec),
        XNA.Point vec => CompareTo((Vector2I)vec),
        XTilePoint vec => CompareTo((Vector2I)vec),
        DrawingSize vec => CompareTo((Vector2I)vec),
        XTileSize vec => CompareTo((Vector2I)vec),
        Tuple<int, int> vector => CompareTo(new Vector2I(vector.Item1, vector.Item2)),
        ValueTuple<int, int> vector => CompareTo(vector),
        _ => Extensions.Exceptions.ThrowArgumentException<int>(nameof(other), other)
    };
}
