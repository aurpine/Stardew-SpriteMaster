﻿using MonoGame.OpenGL;
using SpriteMaster.Extensions;
using SpriteMaster.Harmonize.Patches;
using SpriteMaster.Metadata;
using SpriteMaster.Types;
using StardewModdingAPI;
using System;

namespace SpriteMaster.GL;

internal static partial class Texture2DExt {
    internal static bool CopyTexture(
        XTexture2D source,
        Bounds sourceArea,
        XTexture2D target,
        Bounds targetArea,
        PatchMode patchMode
    ) {
        // Intel drivers on Windows have trouble with glCopyTexSubImage, as seen by Blender's workaround for it.
        if (SystemInfo.Graphics.Vendor is SystemInfo.Graphics.Vendors.Intel && OperatingSystem.IsWindows()) {
            return false;
        }

        if (!SMConfig.Extras.OpenGL.Enabled || !SMConfig.Extras.OpenGL.UseCopyTexture) {
            return false;
        }

        if (!GLExt.CopyImageSubData.Enabled) {
            return false;
        }

        if (patchMode != PatchMode.Replace) {
            return false;
        }

        if (
            target.glFormat is PixelFormat.CompressedTextureFormats ||
            source.glFormat is PixelFormat.CompressedTextureFormats
        ) {
            return false;
        }

        if (!source.TryMeta(out var sourceMeta) || !sourceMeta.HasCachedData) {
            return false;
        }

        if (!source.GetGlMeta().Flags.HasFlag(Texture2DOpenGlMeta.Flag.Initialized)) {
            return false;
        }

        bool targetInitialized = target.GetGlMeta().Flags.HasFlag(Texture2DOpenGlMeta.Flag.Initialized);

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

                // Flush errors
                GLExt.SwallowOrReportErrors();

                if (!targetInitialized) {
                    try {
                        Texture2DExt.InitializeTexture(target);
                    }
                    catch {
                        success = false;
                        return;
                    }
                }

                try {
                    target.CheckTextureMip();
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
