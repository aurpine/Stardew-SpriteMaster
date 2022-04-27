﻿using SpriteMaster.Types;
using System;

namespace SpriteMaster.Configuration.Preview;

class SpriteSheet : MetaTexture {
	internal readonly Vector2I Size;
	internal Vector2I RenderedSize => Size * 4;
	internal readonly Vector2I Dimensions;

	internal Drawable this[int x, int y] {
		get {
			if (x < 0 || x >= Dimensions.X) {
				throw new IndexOutOfRangeException($"Argument '{nameof(x)}' ({x}) is out of range (< 0 or >= {Dimensions.X})");
			}
			if (y < 0 || y >= Dimensions.Y) {
				throw new IndexOutOfRangeException($"Argument '{nameof(y)}' ({y}) is out of range (< 0 or >= {Dimensions.Y})");
			}

			Bounds bounds = new(Size * (x, y), Size);

			return new(Texture, bounds);
		}
	}

	internal SpriteSheet(
		string textureName,
		Vector2I spriteSize,
		Vector2I? dimensions = null
	) : base(textureName) {
		Size = spriteSize;
		Dimensions = dimensions ?? (new Vector2I(Texture.Width, Texture.Height) / spriteSize);
	}
}
