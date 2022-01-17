﻿using SpriteMaster.Extensions;
using SpriteMaster.Types;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#nullable enable

namespace SpriteMaster.Metadata;

static class ReportOnceValidations {
	[Conditional("DEBUG"), MethodImpl(Runtime.MethodImpl.Hot)]
	private static void DebugValidate(in Bounds sourceBounds, XTexture2D referenceTexture) {
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
				Debug.WarningLn($"Out of range source '{source}' for texture '{reference.SafeName()}' ({reference.Width}, {reference.Height})");
		}
		if (source.Right < source.Left || source.Bottom < source.Top) {
			Debug.WarningLn($"Inverted range source '{source}' for texture '{reference.SafeName()}'");
		}
#endif
	}

	[MethodImpl(Runtime.MethodImpl.Hot)]
	internal static void Validate(in Bounds sourceBounds, XTexture2D referenceTexture) {
		DebugValidate(sourceBounds, referenceTexture);
	}

	[Conditional("DEBUG")]
	private static void EmitOverlappingWarning(in Bounds sourceBounds, XTexture2D referenceTexture) {
		if (referenceTexture.Meta().ShouldReportError(ReportOnceErrors.OverlappingSource)) {
			Debug.WarningLn($"Overlapping sprite source '{sourceBounds}' for texture '{referenceTexture.SafeName()}' ({referenceTexture.Extent()})");
		}
	}

	[Conditional("DEBUG")]
	private static void EmitInvertedWarning(in Bounds sourceBounds, XTexture2D referenceTexture) {
		if (referenceTexture.Meta().ShouldReportError(ReportOnceErrors.InvertedSource)) {
			Debug.WarningLn($"Inverted sprite source '{sourceBounds}' for texture '{referenceTexture.SafeName()}' ({referenceTexture.Extent()})");
		}
	}

	[Conditional("DEBUG")]
	private static void EmitDegenerateWarning(in Bounds sourceBounds, XTexture2D referenceTexture) {
		if (referenceTexture.Meta().ShouldReportError(ReportOnceErrors.DegenerateSource)) {
			Debug.WarningLn($"Degenerate sprite source '{sourceBounds}' for texture '{referenceTexture.SafeName()}' ({referenceTexture.Extent()})");
		}
	}
}
