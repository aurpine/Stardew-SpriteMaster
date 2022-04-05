﻿using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nito.Collections;
using Pastel;
using SpriteMaster.Configuration;
using SpriteMaster.Extensions;
using SpriteMaster.Types;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using static SpriteMaster.Harmonize.Harmonize;

namespace SpriteMaster.Harmonize.Patches;

static class TextureCache {
	private const int MaxDequeItems = 20;
	private static readonly Deque<XTexture2D> TextureCacheDeque = new(MaxDequeItems);
	private static readonly ConcurrentDictionary<string, WeakReference<XTexture2D>> TextureCacheTable = new();
	private static readonly ConditionalWeakTable<XTexture2D, string> TexturePaths = new();
	private static readonly WeakSet<XTexture2D> PremultipliedTable = new();
	private static readonly object Lock = new();

	private static readonly Type ModContentManagerType = typeof(StardewModdingAPI.Framework.ModLoading.RewriteFacades.AccessToolsFacade).Assembly.
		GetType("StardewModdingAPI.Framework.ContentManagers.ModContentManager") ?? throw new NullReferenceException("Could not find 'ModContentManager type");

	[Harmonize(
		typeof(XTexture2D),
		"FromStream",
		Harmonize.Fixation.Prefix,
		PriorityLevel.Last,
		platform: Harmonize.Platform.MonoGame,
		instance: false,
		critical: false
	)]
	public static bool FromStreamPre(ref XTexture2D __result, GraphicsDevice graphicsDevice, Stream stream, ref bool __state) {
		lock (Lock) {

			if (!Config.Enabled || !Config.SMAPI.TextureCacheEnabled) {
				__state = false;
				return true;
			}

			if (stream is not FileStream fileStream) {
				__state = false;
				return true;
			}

			bool isContentManager = false;
			var stackTrace = new StackTrace(fNeedFileInfo: false, skipFrames: 1);
			foreach (var frame in stackTrace.GetFrames()) {
				if (frame.GetMethod() is MethodBase method) {
					if (method.DeclaringType == ModContentManagerType) {
						isContentManager = true;
						break;
					}
				}
			}

			if (!isContentManager) {
				__state = false;
				return true;
			}

			__state = true;

			using var watchdogScoped = WatchDog.WatchDog.ScopedWorkingState;

			if (graphicsDevice is null || fileStream is null) {
				return true;
			}

			var path = fileStream.Name;
			if (TextureCacheTable.TryGetValue(path, out var textureRef)) {
				if (textureRef?.TryGetTarget(out var texture) ?? false && texture is not null) {
					if (texture.IsDisposed || texture.GraphicsDevice != graphicsDevice) {
						TextureCacheTable.TryRemove(path, out var _);
						TexturePaths.Remove(texture);
					}
					else {
						Debug.Trace($"Found Texture2D for '{path}' in cache!".Pastel(System.Drawing.Color.LightCyan));
						__result = texture;
						__state = false;
						return false;
					}
				}
			}

			return true;
		}
	}

	[Harmonize(typeof(XTexture2D), "FromStream", Harmonize.Fixation.Postfix, PriorityLevel.Last, platform: Harmonize.Platform.MonoGame, instance: false)]
	public static void FromStreamPost(ref XTexture2D __result, GraphicsDevice graphicsDevice, Stream stream, bool __state) {
		lock (Lock) {
			if (!Config.Enabled || !Config.SMAPI.TextureCacheEnabled) {
				return;
			}

			if (!__state) {
				return;
			}

			using var watchdogScoped = WatchDog.WatchDog.ScopedWorkingState;

			if (__result is null || stream is null) {
				return;
			}

			if (stream is not FileStream fileStream) {
				return;
			}

			var result = __result;

			PremultipliedTable.Remove(result);
		}
	}

	[Harmonize(typeof(XTexture2D), "FromStream", Harmonize.Fixation.Finalizer, PriorityLevel.Last, platform: Harmonize.Platform.MonoGame, instance: false)]
	public static void FromStreamFinal(ref XTexture2D __result, GraphicsDevice graphicsDevice, Stream stream, bool __state) {
		lock (Lock) {
			if (!Config.Enabled || !Config.SMAPI.TextureCacheEnabled) {
				return;
			}

			if (!__state) {
				return;
			}

			using var watchdogScoped = WatchDog.WatchDog.ScopedWorkingState;

			if (__result is null || stream is null) {
				return;
			}

			if (stream is not FileStream fileStream) {
				return;
			}

			var result = __result;
			if (Config.SMAPI.TextureCacheHighMemoryEnabled) {
				lock (TextureCacheDeque) {
					int dequeIndex = TextureCacheDeque.IndexOf(result);
					if (dequeIndex != -1) {
						TextureCacheDeque.RemoveAt(dequeIndex);
					}

					while (TextureCacheDeque.Count >= MaxDequeItems) {
						var texture = TextureCacheDeque.RemoveFromBack();
						PremultipliedTable.Remove(texture);
					}
					TextureCacheDeque.AddToFront(result);
				}
			}
			WeakReference<Texture2D>? previousTexture = null;
			TextureCacheTable.AddOrUpdate(fileStream.Name, result.MakeWeak(), (name, original) => {
				previousTexture = original;
				return result.MakeWeak();
			});
			if (previousTexture?.TryGetTarget(out var previousTextureTarget) ?? false && previousTextureTarget is not null) {
				PremultipliedTable.Remove(previousTextureTarget);
			}
			TexturePaths.AddOrUpdate(result, fileStream.Name);
		}
	}

	private static readonly ThreadLocal<WeakReference<XTexture2D>> CurrentPremultiplyingTexture = new();

	[Harmonize(
		typeof(StardewModdingAPI.Framework.ModLoading.RewriteFacades.AccessToolsFacade),
		"StardewModdingAPI.Framework.ContentManagers.ModContentManager",
		"PremultiplyTransparency",
		Harmonize.Fixation.Prefix,
		Harmonize.PriorityLevel.First
	)]
	public static bool PremultiplyTransparencyPre(ContentManager __instance, ref XTexture2D __result, XTexture2D texture) {
		lock (Lock) {
			if (!Config.Enabled || !Config.SMAPI.TextureCacheEnabled || !Config.SMAPI.PMATextureCacheEnabled) {
				return true;
			}

			if (PremultipliedTable.Contains(texture)) {
				__result = texture;
				CurrentPremultiplyingTexture.Value = null!;
				return false;
			}

			CurrentPremultiplyingTexture.Value = texture.MakeWeak();
			return true;
		}
	}

	[Harmonize(
		typeof(StardewModdingAPI.Framework.ModLoading.RewriteFacades.AccessToolsFacade),
		"StardewModdingAPI.Framework.ContentManagers.ModContentManager",
		"PremultiplyTransparency",
		Harmonize.Fixation.Finalizer,
		Harmonize.PriorityLevel.First
	)]
	public static void PremultiplyTransparencyPost(ContentManager __instance, XTexture2D __result, XTexture2D texture) {
		lock (Lock) {
			CurrentPremultiplyingTexture.Value = null!;

			if (!Config.Enabled || !Config.SMAPI.TextureCacheEnabled || !Config.SMAPI.PMATextureCacheEnabled) {
				return;
			}

			PremultipliedTable.AddOrIgnore(texture);
		}
	}

	internal static void Remove(XTexture2D texture) {
		lock (Lock) {
			// Prevent an annoying circular logic problem
			if (CurrentPremultiplyingTexture.Value?.TryGetTarget(out var currentTexture) ?? false && currentTexture == texture) {
				return;
			}

			PremultipliedTable.Remove(texture);

			if (TexturePaths.TryGetValue(texture, out var path)) {
				TextureCacheTable.TryRemove(path, out var _);
				TexturePaths.Remove(texture);
			}

			lock (TextureCacheDeque) {
				int dequeIndex = TextureCacheDeque.IndexOf(texture);
				if (dequeIndex != -1) {
					TextureCacheDeque.RemoveAt(dequeIndex);
				}
			}
		}
	}

	internal static void Flush(bool reset = false) {
		lock (Lock) {
			TextureCacheDeque.Clear();
			TextureCacheTable.Clear();
			TexturePaths.Clear();
			PremultipliedTable.Clear();
		}
	}
}
