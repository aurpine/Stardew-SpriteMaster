﻿using SpriteMaster.Extensions;
using SpriteMaster.Types;
using System;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Resample.Scalers.xBRZ.Structures;

internal static class Rotator {
    internal const int MaxRotations = 4; // Number of 90 degree rotations
    internal const int MaxPositions = 9;

    // Cache the 4 rotations of the 9 positions, a to i.
    // a = 0, b = 1, c = 2,
    // d = 3, e = 4, f = 5,
    // g = 6, h = 7, i = 8;
    private static readonly FixedArray<int> RotationsArray = new(MaxRotations * MaxPositions);

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static int Get(int index) => RotationsArray[index];

    static Rotator() {
        var rotation = ArrayExt.Range(0, MaxPositions);
        var sideLength = MathF.Sqrt(MaxPositions).RoundToInt();
        for (var rot = 0; rot < MaxRotations; rot++) {
            for (var pos = 0; pos < MaxPositions; pos++) {
                RotationsArray[pos * MaxRotations + rot] = rotation[pos];
            }
            rotation = rotation.RotateClockwise(sideLength);
        }
    }

    //http://stackoverflow.com/a/38964502/294804
    private static int[] RotateClockwise(this int[] square1DMatrix, int sideLength) {
        var size = sideLength;
        var result = new int[square1DMatrix.Length];

        for (var i = 0; i < size; ++i) {
            var offset = i * size;
            for (var j = 0; j < size; ++j) {
                result[offset + j] = square1DMatrix[(size - j - 1) * size + i];
            }
        }

        return result;
    }
}
