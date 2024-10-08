﻿namespace SpriteMaster.Resample.Scalers.xBREPX;

internal sealed class ScalerInfo : IScalerInfo {
    internal static readonly ScalerInfo Instance = new();

    public Resample.Scaler Scaler => Resample.Scaler.xBREPX;
    public int MinScale => 1;
    public int MaxScale => Config.MaxScale;
    public XGraphics.TextureFilter? Filter => XGraphics.TextureFilter.Linear;
    public bool PremultiplyAlpha => true;
    public bool GammaCorrect => true;
    public bool BlockCompress => true;

    public IScaler Interface => xBREPX.Scaler.ScalerInterface.Instance;

    private ScalerInfo() { }
}
