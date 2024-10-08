﻿using SpriteMaster.Extensions;

namespace SpriteMaster.Numeric;

internal static class Float {
    internal static class Half {
        internal const uint Bits = 16;
        internal const uint ExponentBits = 5;
        internal const uint ExponentMask = (1U << (int)ExponentBits) - 1;
        internal const uint ExponentMaskShifted = ExponentMask << (int)SignificandBits;
        internal const uint SignificandBits = 10;
        internal const uint SignificandMask = (1U << (int)SignificandBits) - 1;
        internal const uint MantissaBits = SignificandBits;

        internal static half Make(uint exponent, uint significand) => ((exponent << (int)SignificandBits) | significand).ReinterpretAs<half>();
    }

    internal static class Single {
        internal const uint Bits = 32;
        internal const uint ExponentBits = 8;
        internal const uint ExponentMask = (1U << (int)ExponentBits) - 1;
        internal const uint ExponentMaskShifted = ExponentMask << (int)SignificandBits;
        internal const uint SignificandBits = 23;
        internal const uint SignificandMask = (1U << (int)SignificandBits) - 1;
        internal const uint MantissaBits = SignificandBits;

        internal static float Make(uint exponent, uint significand) => ((exponent << (int)SignificandBits) | significand).ReinterpretAs<float>();
    }

    internal static class Double {
        internal const uint Bits = 64;
        internal const uint ExponentBits = 11;
        internal const ulong ExponentMask = (1UL << (int)ExponentBits) - 1;
        internal const ulong ExponentMaskShifted = ExponentMask << (int)SignificandBits;
        internal const uint SignificandBits = 52;
        internal const ulong SignificandMask = (1UL << (int)SignificandBits) - 1;
        internal const uint MantissaBits = SignificandBits;

        internal static double Make(uint exponent, ulong significand) => (((ulong)exponent << (int)SignificandBits) | significand).ReinterpretAs<double>();
    }
}
