﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SpriteMaster.HarmonyExt.Patches {
	[SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Harmony")]
	[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Harmony")]
	internal static class PGraphicsDevice {
		[HarmonyPatch("Present")]
		internal static bool Present (GraphicsDevice __instance) {
			DrawState.OnPresent();
			return true;
		}

		[HarmonyPatch("Present")]
		internal static bool Present (GraphicsDevice __instance, Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle) {
			DrawState.OnPresent();
			return true;
		}
	}
}
