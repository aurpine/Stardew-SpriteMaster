﻿using Microsoft.Xna.Framework.Graphics;
using SpriteMaster.Extensions;
using SpriteMaster.Metadata;
using SpriteMaster.Types;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Core;

static partial class OnDrawImpl {
	[MethodImpl(Runtime.MethodImpl.Hot)]
	private static bool GetIsSliced(in Bounds bounds, Texture2D reference, [NotNullWhen(true)] out Config.TextureRef? result) {
		var normalizedName = reference.NormalizedName();

		foreach (var slicedTexture in Config.Resample.SlicedTexturesS) {
			if (!normalizedName.StartsWith(slicedTexture.Texture)) {
				continue;
			}
			if (slicedTexture.Bounds.IsEmpty || slicedTexture.Bounds.Contains(bounds)) {
				result = slicedTexture;
				return true;
			}
		}
		result = null;
		return false;
	}

	[MethodImpl(Runtime.MethodImpl.Hot)]
	private static bool Cleanup(this ref Bounds sourceBounds, Texture2D reference) {
		if (Config.ClampInvalidBounds && !sourceBounds.ClampToChecked(reference.Extent(), out var clampedBounds)) {
			//Debug.Warning($"Draw.Cleanup: '{reference.SafeName()}' bounds '{sourceBounds}' are not contained in reference bounds '{(Bounds)reference.Bounds}' - clamped ({(sourceBounds.Degenerate ? "degenerate" : "")})");
			sourceBounds = clampedBounds;
		}

		// Let's just skip potentially invalid draws since I have no idea what to do with them.
		return !sourceBounds.Degenerate;
	}

	[MethodImpl(Runtime.MethodImpl.Hot)]
	private static bool FetchScaledTexture(
		this Texture2D reference,
		uint expectedScale,
		ref Bounds source,
		[NotNullWhen(true)] out ManagedSpriteInstance? spriteInstance,
		bool create = false
	) {
		var invert = source.Invert;
		spriteInstance = reference.FetchScaledTexture(
			expectedScale: expectedScale,
			source: ref source,
			create: create
		);
		source.Invert = invert;
		return spriteInstance is not null;
	}

	private static ManagedSpriteInstance? FetchScaledTexture(
		this Texture2D reference,
		uint expectedScale,
		ref Bounds source,
		bool create = false
	) {
		var clampedSource = source;

		try {
			if (reference is InternalTexture2D) {
				return null;
			}

			// If the (potentially-clamped) source bounds are invalid, return null
			if (!clampedSource.Cleanup(reference)) {
				return null;
			}

			// If the reference texture's dimensions are invalid, return null
			if (reference.Width < 1 || reference.Height < 1) {
				return null;
			}

			// If the reference texture is too small to consider resampling, return null
			if (reference.Extent().MaxOf <= Config.Resample.MinimumTextureDimensions) {
				return null;
			}

			bool isSliced = false;
			if (GetIsSliced(clampedSource, reference, out var textureRef)) {
				clampedSource = textureRef.Value.Bounds;
				isSliced = true;
			}

			var spriteInstance = create ?
				ManagedSpriteInstance.FetchOrCreate(texture: reference, source: clampedSource, expectedScale: expectedScale, sliced: isSliced) :
				ManagedSpriteInstance.Fetch(texture: reference, source: clampedSource, expectedScale: expectedScale);

			if (spriteInstance is null || !spriteInstance.IsReady) {
				return null;
			}

			var t = spriteInstance.Texture!;

			if (!t.Validate()) {
				return null;
			}

			source = t.Dimensions;

			return spriteInstance;
		}
		catch (Exception ex) {
			ex.PrintError();
		}

		return null;
	}

	[MethodImpl(Runtime.MethodImpl.Hot)]
	private static bool Validate(this ManagedTexture2D texture) => !texture.IsDisposed;

	[MethodImpl(Runtime.MethodImpl.Hot)]
	private static void GetDrawParameters(Texture2D texture, in XNA.Rectangle? source, out Bounds bounds, out float scaleFactor) {
		if (texture is not InternalTexture2D) {
			texture.Meta().UpdateLastAccess();
		}

		var sourceRectangle = (Bounds)source.GetValueOrDefault(new(0, 0, texture.Width, texture.Height));

		if (Config.Resample.TrimWater && SpriteOverrides.IsWater(sourceRectangle, texture)) {
			scaleFactor = 4.0f;
		}
		else {
			scaleFactor = 1.0f;
		}

		ReportOnceValidations.Validate(sourceRectangle, texture);
		bounds = sourceRectangle;
	}
}
