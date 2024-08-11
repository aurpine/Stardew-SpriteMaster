using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteMaster.Resample.Scalers.Identity;
internal class ScalerInfo : IScalerInfo {
    internal static readonly ScalerInfo Instance = new();

    public Resample.Scaler Scaler => Resample.Scaler.Vanilla;
    public int MinScale => 1;
    public int MaxScale => 1;
    public XGraphics.TextureFilter? Filter => null;
    public bool PremultiplyAlpha => false;
    public bool GammaCorrect => false;
    public bool BlockCompress => false;

    public IScaler Interface => Identity.Scaler.ScalerInterface.Instance;

    private ScalerInfo() { }
}
