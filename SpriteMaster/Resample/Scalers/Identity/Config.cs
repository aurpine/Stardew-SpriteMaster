using SpriteMaster.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpriteMaster.Resample.Scalers.Identity;

internal class Config : Resample.Scalers.Config {
    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal Config(
        Vector2B wrapped,
        bool hasAlpha,
        bool gammaCorrected) : base(
            wrapped,
            hasAlpha,
            gammaCorrected) {
    }
}
