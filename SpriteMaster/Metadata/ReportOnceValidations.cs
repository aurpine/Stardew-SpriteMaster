﻿using SpriteMaster.Extensions;
using SpriteMaster.Types;
using System.Diagnostics;

namespace SpriteMaster.Metadata;

internal static class ReportOnceValidations {
    [Conditional("DEBUG")]
    private static void DebugValidate(Bounds sourceBounds, XTexture2D referenceTexture) {
        Bounds referenceBounds = referenceTexture.Bounds;

        if (!referenceBounds.Contains(sourceBounds)) {
            EmitOverlappingWarning(sourceBounds, referenceTexture);
        }

        if (sourceBounds.Right < sourceBounds.Left || sourceBounds.Bottom < sourceBounds.Top) {
            EmitInvertedWarning(sourceBounds, referenceTexture);
        }

        if (sourceBounds.Degenerate) {
            EmitDegenerateWarning(sourceBounds, referenceTexture);
        }
#if false
		if (source.Left < 0 || source.Top < 0 || source.Right >= reference.Width || source.Bottom >= reference.Height) {
			if (source.Right - reference.Width > 1 || source.Bottom - reference.Height > 1)
				Debug.Warning($"Out of range source '{source}' for texture '{reference.SafeName()}' ({reference.Width}, {reference.Height})");
		}
		if (source.Right < source.Left || source.Bottom < source.Top) {
			Debug.Warning($"Inverted range source '{source}' for texture '{reference.SafeName()}'");
		}
#endif
    }

    [Conditional("DEBUG")]
    internal static void Validate(Bounds sourceBounds, XTexture2D referenceTexture) {
        DebugValidate(sourceBounds, referenceTexture);
    }

    [Conditional("DEBUG")]
    private static void EmitOverlappingWarning(Bounds sourceBounds, XTexture2D referenceTexture) {
        if (referenceTexture is not InternalTexture2D && referenceTexture.Meta().ShouldReportError(ReportOnceErrors.OverlappingSource)) {
            Debug.Warning($"Overlapping sprite source '{sourceBounds}' for texture '{referenceTexture.NormalizedName()}' ({referenceTexture.Extent()})");
        }
    }

    [Conditional("DEBUG")]
    private static void EmitInvertedWarning(Bounds sourceBounds, XTexture2D referenceTexture) {
        if (referenceTexture is not InternalTexture2D && referenceTexture.Meta().ShouldReportError(ReportOnceErrors.InvertedSource)) {
            Debug.Warning($"Inverted sprite source '{sourceBounds}' for texture '{referenceTexture.NormalizedName()}' ({referenceTexture.Extent()})");
        }
    }

    [Conditional("DEBUG")]
    private static void EmitDegenerateWarning(Bounds sourceBounds, XTexture2D referenceTexture) {
        if (referenceTexture is not InternalTexture2D && referenceTexture.Meta().ShouldReportError(ReportOnceErrors.DegenerateSource)) {
            Debug.Warning($"Degenerate sprite source '{sourceBounds}' for texture '{referenceTexture.NormalizedName()}' ({referenceTexture.Extent()})");
        }
    }
}
