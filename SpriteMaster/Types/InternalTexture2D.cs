﻿using Microsoft.Xna.Framework.Graphics;

#nullable enable

namespace SpriteMaster.Types;

/// <summary>
/// A Texture2D that represents internal SpriteMaster data, and thus shouldn't continue down any resampling pipelines
/// </summary>
abstract class InternalTexture2D : XNA.Graphics.Texture2D {
	private const string DefaultName = "Texture (Internal)";

	internal InternalTexture2D(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height) {
		Name = DefaultName;
	}

	internal InternalTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format) : base(graphicsDevice, width, height, mipmap, format) {
		Name = DefaultName;
	}

	internal InternalTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize) : base(graphicsDevice, width, height, mipmap, format, arraySize) {
		Name = DefaultName;
	}

	protected InternalTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared, int arraySize) : base(graphicsDevice, width, height, mipmap, format, type, shared, arraySize) {
		Name = DefaultName;
	}
}