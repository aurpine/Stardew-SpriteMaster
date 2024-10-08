﻿using SpriteMaster.Extensions;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpriteMaster.Types;

[CLSCompliant(false)]
[DebuggerDisplay("[{Min} <-> {Max}}")]
[StructLayout(LayoutKind.Sequential, Pack = Vector2I.Alignment, Size = Vector2I.ByteSize)]
internal readonly struct ExtentI : IEquatable<ExtentI>, ILongHash {
    private readonly Vector2I Value;

    internal readonly int Min => Value.X;
    internal readonly int Max => Value.Y;

    internal readonly bool IsValid => Min <= Max;

    internal readonly int Length => Max - Min;

    internal ExtentI(int min, int max) : this() {
        min.AssertLessEqual(max);
        Value = new(min, max);
    }

    internal ExtentI(ExtentI value) : this(value.Min, value.Max) { }

    internal ExtentI((int Min, int Max) value) : this(value.Min, value.Max) { }

    internal readonly bool ContainsInclusive(int value) => value.WithinInclusive(Min, Max);

    internal readonly bool ContainsExclusive(int value) => value.WithinExclusive(Min, Max);

    internal readonly bool Contains(int value) => value.Within(Min, Max);

    internal readonly bool ContainsInclusive(ExtentI value) => value.Min >= Min && value.Max <= Max;

    internal readonly bool ContainsExclusive(ExtentI value) => value.Min > Min && value.Max < Max;

    internal readonly bool Contains(ExtentI value) => ContainsInclusive(value);

    public bool Equals(ExtentI other) => Value == other.Value;

    public override readonly bool Equals(object? other) {
        switch (other) {
            case ExtentI value: return Equals(value);
            case ValueTuple<int, int> value: return Equals(new ExtentI(value));
            default: return false;
        }
    }

    public override readonly int GetHashCode() => Value.GetHashCode();

    public readonly ulong GetLongHashCode() => Value.GetLongHashCode();

    public static bool operator ==(ExtentI left, ExtentI right) => Equals(left, right);

    public static bool operator !=(ExtentI left, ExtentI right) => !Equals(left, right);
}
