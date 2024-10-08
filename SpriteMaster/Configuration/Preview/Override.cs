﻿namespace SpriteMaster.Configuration.Preview;

internal class Override {
    internal static Override? Instance = null;

    internal bool Enabled = false;
    internal bool ResampleEnabled = false;
    internal Resample.Scaler Scaler = Resample.Scaler.None;
    internal Resample.Scaler ScalerPortrait = Resample.Scaler.None;
    internal Resample.Scaler ScalerText = Resample.Scaler.None;
    internal Resample.Scaler ScalerGradient = Resample.Scaler.None;
    internal bool ResampleSprites = false;
    internal bool ResamplePortraits = false;
    internal bool ResampleLargeText = false;
    internal bool ResampleSmallText = false;
    internal bool FiltersEnabled = false;
    internal int Saturation = 0;
    internal int Brightness = 0;
    internal int Temperature = 0;

    // draw state
    internal bool SetLinearUnresampled = false;
    internal bool SetLinear = true;

#pragma warning disable CS0618 // Type or member is obsolete
    internal static Override FromConfig => new() {
        Enabled = Config.IsUnconditionallyEnabled,
        ResampleEnabled = Config.Resample.Enabled,
        Scaler = Config.Resample.Scaler,
        ScalerPortrait = Config.Resample.ScalerPortrait,
        ScalerText = Config.Resample.ScalerText,
        ScalerGradient = Config.Resample.ScalerGradient,
        ResampleSprites = Config.Resample.EnabledSprites,
        ResamplePortraits = Config.Resample.EnabledPortraits,
        ResampleLargeText = Config.Resample.EnabledLargeText,
        ResampleSmallText = Config.Resample.EnabledSmallText,
        FiltersEnabled = Config.Resample.Filters.Enabled,
        Saturation = Config.Resample.Filters.Saturation,
        Brightness = Config.Resample.Filters.Brightness,
        Temperature = Config.Resample.Filters.Temperature,

        SetLinearUnresampled = Config.DrawState.SetLinearUnresampled,
        SetLinear = Config.DrawState.SetLinear
    };
#pragma warning restore CS0618 // Type or member is obsolete
}
