﻿using StardewValley;
using System.Runtime.CompilerServices;
using System.Threading;
using SMHarmonize = SpriteMaster.Harmonize;

namespace SpriteMaster.Configuration.Preview;

internal static class PrecipitationPatches {
    private static PrecipitationType Precipitation => PrecipitationOverride ?? Scene.Current?.Precipitation ?? PrecipitationType.None;
    internal static PrecipitationType? PrecipitationOverride = null;

    // This is here because the M1 seems to have issues with reverse patches.
    internal static readonly ThreadLocal<bool> IsReverse = new(false);

    [MethodImpl(MethodImplOptions.NoInlining)]
    [SMHarmonize.Harmonize(
        typeof(Game1),
        "IsSnowingHere",
        SMHarmonize.Harmonize.Fixation.ReversePatched,
        instance: false,
        critical: false
    )]
    public static bool IsSnowingHereReverse(GameLocation? location) {
        try {
            IsReverse.Value = true;
            return Game1.IsSnowingHere(location);
        }
        finally {
            IsReverse.Value = false;
        }
    }

    public static bool IsSnowingHereExt(GameLocation? location = null) {
        if (Precipitation != PrecipitationType.Snow) {
            if (Scene.Current is null) {
                return IsSnowingHereReverse(location);
            }

            return false;
        }

        if (ReferenceEquals(location, Scene.SceneLocation.Value) || location is null) {
            return true;
        }

        return IsSnowingHereReverse(location);
    }

    [SMHarmonize.Harmonize(
        typeof(Game1),
        "IsSnowingHere",
        SMHarmonize.Harmonize.Fixation.Prefix,
        SMHarmonize.Harmonize.PriorityLevel.Last,
        instance: false,
        critical: false
    )]
    public static bool IsSnowingHere(ref bool __result, GameLocation? location) {
        if (IsReverse.Value) {
            return true;
        }

        if (!SMConfig.IsEnabled || !SMConfig.Extras.Snow.IsEnabled) {
            if (Precipitation == PrecipitationType.Snow) {
                __result = true;
                return false;
            }

            return true;
        }
        return false;
    }

    [SMHarmonize.Harmonize(
        typeof(Game1),
        "IsRainingHere",
        SMHarmonize.Harmonize.Fixation.Prefix,
        SMHarmonize.Harmonize.PriorityLevel.Last,
        instance: false,
        critical: false
    )]
    public static bool IsRainingHere(ref bool __result, GameLocation? location) {
        if (Precipitation != PrecipitationType.Rain) {
            if (Scene.Current is null) {
                return true;
            }
            else {
                __result = false;
                return false;
            }
        }

        if (ReferenceEquals(location, Scene.SceneLocation.Value) || location is null) {
            __result = true;
            return false;
        }

        return true;
    }
}
