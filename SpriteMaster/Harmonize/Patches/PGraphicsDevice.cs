﻿using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics.CodeAnalysis;
using static SpriteMaster.Harmonize.Harmonize;

namespace SpriteMaster.Harmonize.Patches;

[SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Harmony")]
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Harmony")]
internal static class PGraphicsDevice {
	#region Present

	//[Harmonize("Present", fixation: Harmonize.Fixation.Postfix, priority: PriorityLevel.Last, critical: false)]
	//internal static void PresentPost(GraphicsDevice __instance, in XRectangle? sourceRectangle, in XRectangle? destinationRectangle, IntPtr overrideWindowHandle) => DrawState.OnPresentPost();

	[Harmonize("Present", fixation: Harmonize.Fixation.Prefix, priority: PriorityLevel.Last)]
	public static void PresentPre(GraphicsDevice __instance) {
		DrawState.OnPresent();
	}

	[Harmonize("Present", fixation: Harmonize.Fixation.Postfix, priority: PriorityLevel.Last)]
	public static void PresentPost(GraphicsDevice __instance) {
		DrawState.OnPresentPost();
	}

	#endregion

	#region Reset

	[Harmonize("Reset", fixation: Harmonize.Fixation.Postfix, priority: PriorityLevel.Last)]
	public static void OnResetPost(GraphicsDevice __instance) {
		DrawState.OnPresentPost();
	}

	#endregion
}
