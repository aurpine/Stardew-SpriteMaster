﻿using Microsoft.Xna.Framework.Graphics;

#nullable enable

namespace SpriteMaster.Types;

/// <summary>
/// A Texture2D that represents a texture being used to dump a PNG
/// </summary>
sealed class DumpTexture2D : InternalTexture2D {
	private const string DefaultName = "Dump Texture (Internal)";

	internal DumpTexture2D(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height) {
		Name = DefaultName;
	}

	internal DumpTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format) : base(graphicsDevice, width, height, mipmap, format) {
		Name = DefaultName;
	}

	internal DumpTexture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize) : base(graphicsDevice, width, height, mipmap, format, arraySize) {
		Name = DefaultName;
	}
}