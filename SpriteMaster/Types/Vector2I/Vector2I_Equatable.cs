﻿using System;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Types;

internal partial struct Vector2I :
    IEquatable<Vector2I>,
    IEquatable<Vector2I?>,
    IEquatable<(int, int)>,
    IEquatable<(int, int)?>,
    IEquatable<DrawingPoint>,
    IEquatable<DrawingPoint?>,
    IEquatable<XNA.Point>,
    IEquatable<XNA.Point?>,
    IEquatable<XTilePoint>,
    IEquatable<XTilePoint?>,
    IEquatable<DrawingSize>,
    IEquatable<DrawingSize?>,
    IEquatable<XTileSize>,
    IEquatable<XTileSize?> {

    #region Equals

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public override readonly bool Equals(object? other) => other switch {
        Vector2I vec => Equals(vec),
        DrawingPoint vec => Equals(vec),
        XNA.Point vec => Equals(vec),
        XTilePoint vec => Equals(vec),
        DrawingSize vec => Equals(vec),
        XTileSize vec => Equals(vec),
        Tuple<int, int> vector => Equals(new Vector2F(vector.Item1, vector.Item2)),
        ValueTuple<int, int> vector => Equals(vector),
        _ => false,
    };

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly bool Equals(Vector2I other) => Packed == other.Packed;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly bool Equals(Vector2I? other) => other is not null && Equals(other.Value);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly bool Equals((int, int) other) => this == (Vector2I)other;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly bool Equals((int, int)? other) => other is not null && this == (Vector2I)other.Value;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly bool Equals(DrawingPoint other) => this == (Vector2I)other;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly bool Equals(DrawingPoint? other) => other is not null && this == (Vector2I)other.Value;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly bool Equals(XNA.Point other) => this == (Vector2I)other;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly bool Equals(XNA.Point? other) => other is not null && this == (Vector2I)other.Value;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly bool Equals(XTilePoint other) => this == (Vector2I)other;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly bool Equals(XTilePoint? other) => other is not null && this == (Vector2I)other.Value;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly bool Equals(DrawingSize other) => this == (Vector2I)other;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly bool Equals(DrawingSize? other) => other is not null && this == (Vector2I)other.Value;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly bool Equals(XTileSize other) => this == (Vector2I)other;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public readonly bool Equals(XTileSize? other) => other is not null && this == (Vector2I)other.Value;

    #endregion

    #region == and !=

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator ==(Vector2I lhs, Vector2I rhs) => lhs.Packed == rhs.Packed;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator !=(Vector2I lhs, Vector2I rhs) => lhs.Packed != rhs.Packed;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator ==(Vector2I lhs, (int X, int Y) rhs) => lhs.Equals(rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator !=(Vector2I lhs, (int X, int Y) rhs) => !(lhs == rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator ==((int X, int Y) lhs, Vector2I rhs) => rhs.Equals(lhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator !=((int X, int Y) lhs, Vector2I rhs) => !(lhs == rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator ==(Vector2I lhs, DrawingPoint rhs) => lhs.Equals(rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator !=(Vector2I lhs, DrawingPoint rhs) => !(lhs == rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator ==(DrawingPoint lhs, Vector2I rhs) => rhs.Equals(lhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator !=(DrawingPoint lhs, Vector2I rhs) => !(lhs == rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator ==(Vector2I lhs, XNA.Point rhs) => lhs.Equals(rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator !=(Vector2I lhs, XNA.Point rhs) => !(lhs == rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator ==(XNA.Point lhs, Vector2I rhs) => rhs.Equals(lhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator !=(XNA.Point lhs, Vector2I rhs) => !(lhs == rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator ==(Vector2I lhs, XTilePoint rhs) => lhs.Equals(rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator !=(Vector2I lhs, XTilePoint rhs) => !(lhs == rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator ==(XTilePoint lhs, Vector2I rhs) => rhs.Equals(lhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator !=(XTilePoint lhs, Vector2I rhs) => !(lhs == rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator ==(Vector2I lhs, DrawingSize rhs) => lhs.Equals(rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator !=(Vector2I lhs, DrawingSize rhs) => !(lhs == rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator ==(DrawingSize lhs, Vector2I rhs) => rhs.Equals(lhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator !=(DrawingSize lhs, Vector2I rhs) => !(lhs == rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator ==(Vector2I lhs, XTileSize rhs) => lhs.Equals(rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator !=(Vector2I lhs, XTileSize rhs) => !(lhs == rhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator ==(XTileSize lhs, Vector2I rhs) => rhs.Equals(lhs);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    public static bool operator !=(XTileSize lhs, Vector2I rhs) => !(lhs == rhs);

    #endregion
}
