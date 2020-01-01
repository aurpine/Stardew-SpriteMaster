﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace SpriteMaster.HarmonyExt.Patches.PSpriteBatch {
	static class Begin {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[HarmonyPatch("Begin", fixation: HarmonyPatch.Fixation.Postfix, priority: HarmonyExt.PriorityLevel.Last)]
		internal static void OnBegin (SpriteBatch __instance, SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformMatrix) {
			DrawState.OnBegin(__instance, sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
		}
	}
}
