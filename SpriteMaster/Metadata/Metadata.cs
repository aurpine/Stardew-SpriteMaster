﻿using Microsoft.Xna.Framework.Graphics;
using SpriteMaster.Types;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SpriteMaster.Metadata;

static class Metadata {
	private static readonly ConditionalWeakTable<Texture2D, Texture2DMeta> Texture2DMetaTable = new();

	[MethodImpl(Runtime.MethodImpl.Hot)]
	internal static Texture2DMeta Meta(this Texture2D @this) {
#if DEBUG
		if (@this is InternalTexture2D) {
			Debugger.Break();
		}
#endif
		return Texture2DMetaTable.GetValue(@this, key => new(key));
	}

	[MethodImpl(Runtime.MethodImpl.Hot)]
	internal static bool TryMeta(this Texture2D @this, out Texture2DMeta? value) => Texture2DMetaTable.TryGetValue(@this, out value);
}

