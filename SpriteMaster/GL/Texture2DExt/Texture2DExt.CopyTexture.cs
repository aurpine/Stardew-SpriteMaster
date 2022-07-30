﻿using MonoGame.OpenGL;
using SpriteMaster.Extensions;
using SpriteMaster.Harmonize.Patches;
using SpriteMaster.Metadata;
using SpriteMaster.Types;
using StardewModdingAPI;

namespace SpriteMaster.GL;

internal static partial class Texture2DExt {
	internal static bool CopyTexture(
		XTexture2D source,
		Bounds sourceArea,
		XTexture2D target,
		Bounds targetArea,
		PatchMode patchMode
	) {
		if (!Configuration.Config.Extras.OptimizeOpenGL || !Configuration.Config.Extras.UseCopyTexture) {
			return false;
		}

		if (!GLExt.CopyImageSubData.Enabled) {
			return false;
		}

		if (patchMode != PatchMode.Replace) {
			return false;
		}

		if (!source.TryMeta(out var meta) || !meta.HasCachedData) {
			return false;
		}

		if (!source.GetGlMeta().Flags.HasFlag(Texture2DOpenGlMeta.Flag.Initialized)) {
			return false;
		}

		if (!target.GetGlMeta().Flags.HasFlag(Texture2DOpenGlMeta.Flag.Initialized)) {
			return false;
		}

		// We have to perform an internal SetData to make sure SM's caches are kept intact
		var cachedData = PTexture2D.GetCachedData<byte>(
			__instance: source,
			level: 0,
			arraySlice: 0,
			rect: sourceArea,
			data: default
		);

		if (cachedData.IsEmpty) {
			return false;
		}

		bool success = true;

		ThreadingExt.ExecuteOnMainThread(
			() => {
				using var reboundTexture = new TextureBinder(0);

				GLExt.AlwaysCheckError();
				try {
					GLExt.AlwaysChecked(
						() => GLExt.CopyImageSubData.Function(
							(GLExt.ObjectId)source.glTexture,
							TextureTarget.Texture2D,
							0,
							sourceArea.X,
							sourceArea.Y,
							0,
							(GLExt.ObjectId)target.glTexture,
							TextureTarget.Texture2D,
							0,
							targetArea.X,
							targetArea.Y,
							0,
							(uint)sourceArea.Width,
							(uint)sourceArea.Height,
							1u
						)
					);
				}
				catch {
					GLExt.CopyImageSubData.Disable();
					success = false;
				}
			}
		);

		if (!success) {
			return false;
		}

		PTexture2D.OnPlatformSetDataPostInternal<byte>(
			__instance: target,
			level: 0,
			arraySlice: 0,
			rect: targetArea,
			data: cachedData
		);

		return true;
	}
}