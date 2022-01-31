﻿using Microsoft.Xna.Framework.Graphics;
using SpriteMaster.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using static SpriteMaster.Harmonize.Harmonize;
using SpriteBatcher = System.Object;

namespace SpriteMaster.Harmonize.Patches.PSpriteBatch;

[SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Harmony")]
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Harmony")]
static class PlatformRenderBatch {
	private static SamplerState GetNewSamplerState(Texture texture, SamplerState reference) {
		if (!Config.DrawState.SetLinear) {
			return reference;
		}

		if (texture is ManagedTexture2D managedTexture/* && managedTexture.Texture != null*/) {
			return SamplerState.LinearClamp;
		}

		return reference;
	}

	[Harmonize(
		"Microsoft.Xna.Framework.Graphics",
		"Microsoft.Xna.Framework.Graphics.SpriteBatcher",
		"FlushVertexArray",
		Harmonize.Fixation.Prefix,
		PriorityLevel.First,
		platform: Harmonize.Platform.MonoGame
	)]
	internal static bool OnFlushVertexArray(
		SpriteBatcher __instance,
		int start,
		int end,
		Effect effect,
		Texture texture,
		GraphicsDevice ____device,
		ref SamplerState? __state
	) {
		try {
			using var watchdogScoped = WatchDog.WatchDog.ScopedWorkingState;

			var originalState = ____device?.SamplerStates[0] ?? SamplerState.PointClamp;

			var newState = GetNewSamplerState(texture, originalState);

			if (newState != originalState && ____device?.SamplerStates != null) {
				__state = originalState;
				____device.SamplerStates[0] = newState;
			}
			else {
				__state = null;
			}
		}
		catch (Exception ex) {
			ex.PrintError();
		}

		return true;
	}

	[Harmonize(
	"Microsoft.Xna.Framework.Graphics",
	"Microsoft.Xna.Framework.Graphics.SpriteBatcher",
	"FlushVertexArray",
	Harmonize.Fixation.Postfix,
	PriorityLevel.Last,
	platform: Harmonize.Platform.MonoGame
)]
	internal static void OnFlushVertexArray(
	SpriteBatcher __instance,
	int start,
	int end,
	Effect effect,
	Texture texture,
	GraphicsDevice ____device,
	SamplerState? __state
) {
		try {
			using var watchdogScoped = WatchDog.WatchDog.ScopedWorkingState;

			if (__state is not null && ____device?.SamplerStates != null && __state != ____device.SamplerStates[0]) {
				____device.SamplerStates[0] = __state;
			}
		}
		catch (Exception ex) {
			ex.PrintError();
		}
	}
}
