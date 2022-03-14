﻿using Microsoft.Xna.Framework.Graphics;
using SpriteMaster.Extensions;
using SpriteMaster.Metadata;
using SpriteMaster.Types;
using SpriteMaster.Types.Fixed;
using StardewModdingAPI;
using System;
using System.Reflection;

namespace SpriteMaster.Harmonize.Patches.SMAPI;

static class PAssetDataForImage {
	private const int MaxStackSize = 256;

	private static readonly Assembly? ReferenceAssembly = typeof(StardewModdingAPI.Framework.ModLoading.RewriteFacades.AccessToolsFacade)?.Assembly;
	private static readonly Type? AssetDataForImageType = ReferenceAssembly?.GetType("StardewModdingAPI.Framework.Content.AssetDataForImage");
	private static readonly byte MinOpacity = ((byte?)AssetDataForImageType?.GetField("MinOpacity", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null)) ?? 5;

	[Harmonize(
		typeof(StardewModdingAPI.Framework.ModLoading.RewriteFacades.AccessToolsFacade),
		"StardewModdingAPI.Framework.Content.AssetDataForImage",
		"PatchImage",
		Harmonize.Fixation.Prefix,
		Harmonize.PriorityLevel.Last,
		critical: false
	)]
	public static unsafe bool PatchImage(IAssetDataForImage __instance, Texture2D source, XNA.Rectangle? sourceArea, XNA.Rectangle? targetArea, PatchMode patchMode) {
		if (!Config.SMAPI.ApplyPatchEnabled) {
			return true;
		}

		// get texture
		if (source is null) {
			throw new ArgumentNullException(nameof(source), "Can't patch from a null source texture.");
		}
		Texture2D target = __instance.Data;

		// get areas
		sourceArea ??= new(0, 0, source.Width, source.Height);
		targetArea ??= new(0, 0, Math.Min(sourceArea.Value.Width, target.Width), Math.Min(sourceArea.Value.Height, target.Height));

		// validate
		if (!source.Bounds.Contains(sourceArea.Value))
			throw new ArgumentOutOfRangeException(nameof(sourceArea), "The source area is outside the bounds of the source texture.");
		if (!target.Bounds.Contains(targetArea.Value))
			throw new ArgumentOutOfRangeException(nameof(targetArea), "The target area is outside the bounds of the target texture.");
		if (sourceArea.Value.Size != targetArea.Value.Size)
			throw new InvalidOperationException("The source and target areas must be the same size.");

		// get source data
		int pixelCount = sourceArea.Value.Width * sourceArea.Value.Height;
		var sourceData = GC.AllocateUninitializedArray<XNA.Color>(pixelCount);
		source.GetData(0, sourceArea, sourceData, 0, pixelCount);

		static byte[] GetTextureData(Texture2D texture, in Bounds bounds, int count) {
			count *= sizeof(Color8);
			var dataArray = GC.AllocateUninitializedArray<byte>(count, pinned: Config.SMAPI.ApplyPatchPinMemory);
			texture.GetData(0, bounds, dataArray, 0, count);
			return dataArray;
		}

		// merge data in overlay mode
		if (patchMode == PatchMode.Overlay) {
			// get target data
			var targetData = GC.AllocateUninitializedArray<XNA.Color>(pixelCount);
			target.GetData(0, targetArea, targetData, 0, pixelCount);

			// merge pixels
			for (int i = 0; i < sourceData.Length; i++) {
				var above = (Color8)sourceData[i];
				var below = (Color8)targetData[i];

				// shortcut transparency
				if (above.A.Value < MinOpacity) {
					sourceData[i] = below;
					continue;
				}
				if (below.A.Value < MinOpacity) {
					sourceData[i] = above;
					continue;
				}

				// merge pixels
				// This performs a conventional alpha blend for the pixels, which are already
				// premultiplied by the content pipeline. The formula is derived from
				// https://shawnhargreaves.com/blog/premultiplied-alpha.html.
				Fixed8 alphaBelow = (byte)(byte.MaxValue - above.A);
				sourceData[i] = new Color8(
					r: above.R.AddClamped(below.R * alphaBelow),
					g: above.G.AddClamped(below.G * alphaBelow),
					b: above.B.AddClamped(below.B * alphaBelow),
					a: MathExt.Max(above.A, below.A)
				);
			}
		}

		// patch target texture
		target.SetData(0, targetArea, sourceData, 0, pixelCount);
		return false;
	}
}
