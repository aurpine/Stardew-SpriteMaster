namespace SpriteMaster.Resample;

internal enum Scaler : int {
    None = -1,
    xBRZ = 0,
#if !SHIPPING
    SuperXBR,
#endif
    Vanilla,
    EPX,
    ScaleX = EPX,
    EPXLegacy,
    xBREPX
}
