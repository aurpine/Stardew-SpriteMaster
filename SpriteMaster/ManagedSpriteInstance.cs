﻿using LinqFasterer;
using Microsoft.Xna.Framework.Graphics;
using Pastel;
using SpriteMaster.Caching;
using SpriteMaster.Configuration;
using SpriteMaster.Extensions;
using SpriteMaster.Extensions.Reflection;
using SpriteMaster.Metadata;
using SpriteMaster.Resample;
using SpriteMaster.Tasking;
using SpriteMaster.Types;
using SpriteMaster.Types.Interlocking;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static SpriteMaster.ResourceManager;
using WeakInstance = System.WeakReference<SpriteMaster.ManagedSpriteInstance>;
using WeakTexture = System.WeakReference<Microsoft.Xna.Framework.Graphics.Texture2D>;

namespace SpriteMaster;

internal sealed class ManagedSpriteInstance : IByteSize, IDisposable {
	private static readonly ConcurrentLinkedListSlim<WeakInstance> RecentAccessList = new();

	[MethodImpl(Runtime.MethodImpl.Inline)]
	private static bool HasLegalFormat(XTexture2D texture) => AllowedFormats.ContainsFast(texture.Format);

	private static void PurgeInvalidated(XTexture2D texture) {
		// If somehow it passed validation earlier (like a SetData before a name) make sure no cached data
		// or handles are left dangling around.

		var meta = texture.Meta();
		meta.Purge();
	}

	internal static ulong GetHash(SpriteInfo info, TextureType type) => Resampler.GetHash(info, type);

	internal static ulong? GetHash(in SpriteInfo.Initializer info, TextureType type) => Resampler.GetHash(in info, type);

	private static readonly Type? PyTKMappedTexture2DType = ReflectionExt.GetTypeExt("PyTK.Types.MappedTexture2D");
	private static readonly Type? PyTKScaledTexture2DType = ReflectionExt.GetTypeExt("PyTK.Types.ScaledTexture2D");

	private static readonly Func<object, Dictionary<XRectangle?, Texture2D>>? GetTextureMap = PyTKMappedTexture2DType?.GetMemberGetter<object, Dictionary<XRectangle?, Texture2D>>("Map");
	private static readonly Func<object, Texture2D>? GetScaledTextureUnderlying = PyTKScaledTexture2DType?.GetMemberGetter<object, Texture2D>("STexture");

	private static XTexture2D GetUnderlyingTexture(XTexture2D texture) {
		if (GetTextureMap is not null && PyTKMappedTexture2DType is { } mappedTexture2DType && texture.GetType().IsAssignableTo(mappedTexture2DType)) {
			if (GetTextureMap(texture) is {} map) {
				return map.FirstOrDefault().Value ?? texture;
			}
		}
		if (GetScaledTextureUnderlying is not null && PyTKScaledTexture2DType is { } scaledTexture2DType && texture.GetType().IsAssignableTo(scaledTexture2DType)) {
			return GetScaledTextureUnderlying(texture) ?? texture;
		}

		return texture;
	}

	internal static bool Validate(XTexture2D texture, bool clean = false) {
		var meta = texture.Meta();
		if (meta.Validation.HasValue) {
			return meta.Validation.Value;
		}

		texture = GetUnderlyingTexture(texture);

		if (texture is InternalTexture2D) {
			if (!meta.TracePrinted) {
				meta.TracePrinted = true;
				Debug.Trace($"Not Scaling Texture '{texture.NormalizedName(DrawingColor.LightYellow)}', Is Internal Texture");
			}
			meta.Validation = false;
			if (clean) {
				PurgeInvalidated(texture);
			}
			return false;
		}

		if (texture is RenderTarget2D && (
				StardewValley.GameRunner.instance.gameInstances.AnyF(game => texture == game.screen || texture == game.uiScreen) ||
				texture.Name is ("UI Screen" or "Screen") ||
				meta.IsSystemRenderTarget
			)
		) {
			if (!meta.TracePrinted) {
				meta.TracePrinted = true;
				//Debug.Trace($"Not Scaling Texture '{texture.NormalizedName(DrawingColor.LightYellow)}', Is System Render Target");
			}
			meta.Validation = false;
			if (clean) {
				PurgeInvalidated(texture);
			}
			return false;
		}

		// For now, render targets are disabled. General ones _can_ be made to work - they need to be invalidated once they are bound to a render target slot,
		// and a hold set on using them until they are no longer bound to a render target slot. Or on clear calls. If the render target is not a persist-type,
		// it should be processed _immediately_. If it is, it should be processed synchronously on first use.
		if (texture is RenderTarget2D) {
			if (!meta.TracePrinted) {
				meta.TracePrinted = true;
				Debug.Trace($"Not Scaling Texture '{texture.NormalizedName(DrawingColor.LightYellow)}', Is Render Target");
			}
			meta.Validation = false;
			if (clean) {
				PurgeInvalidated(texture);
			}
			return false;
		}

		if (Math.Max(texture.Width, texture.Height) <= Config.Resample.MinimumTextureDimensions) {
			if (!meta.TracePrinted) {
				meta.TracePrinted = true;
				Debug.Trace($"Not Scaling Texture '{texture.NormalizedName(DrawingColor.LightYellow)}', Is Too Small: ({texture.Extent().ToString(DrawingColor.Orange)} <= {Config.Resample.MinimumTextureDimensions.ToString(DrawingColor.Orange)})");
			}
			meta.Validation = false;
			if (clean) {
				PurgeInvalidated(texture);
			}
			return false;
		}

		if (texture.Area() == 0) {
			if (!meta.TracePrinted) {
				meta.TracePrinted = true;
				Debug.Trace($"Not Scaling Texture '{texture.NormalizedName(DrawingColor.LightYellow)}', Zero Area (Degenerate)");
			}
			meta.Validation = false;
			if (clean) {
				PurgeInvalidated(texture);
			}
			return false;
		}

		// TODO pComPtr check?
		if (texture.IsDisposed || texture.GraphicsDevice.IsDisposed) {
			if (!meta.TracePrinted) {
				meta.TracePrinted = true;
				Debug.Trace($"Not Scaling Texture '{texture.NormalizedName(DrawingColor.LightYellow)}', Is Zombie");
			}
			meta.Validation = false;
			if (clean) {
				PurgeInvalidated(texture);
			}
			return false;
		}

		if (texture.LevelCount > 1) {
			if (!meta.TracePrinted) {
				meta.TracePrinted = true;
				Debug.Trace($"Not Scaling Texture '{texture.NormalizedName(DrawingColor.LightYellow)}', Multi-Level Textures Unsupported: {texture.LevelCount.ToString(DrawingColor.Orange)} levels");
			}
			meta.Validation = false;
			return false;
		}

		if (!HasLegalFormat(texture)) {
			if (!meta.TracePrinted) {
				meta.TracePrinted = true;
				Debug.Trace($"Not Scaling Texture '{texture.NormalizedName(DrawingColor.LightYellow)}', Format Unsupported: {texture.Format.ToString(DrawingColor.Orange)}");
			}
			meta.Validation = false;
			if (clean) {
				PurgeInvalidated(texture);
			}
			return false;
		}

		bool disableValidation = false;

		bool IsLargeFont(Texture2D texture) {
			if (
				texture.NormalizedName().StartsWith(@"Fonts\") &&
				(
					!texture.NormalizedName().Contains("Small") ||
					!texture.NormalizedName().Contains("tiny")
				)
			) {
				return true;
			}

			if (Game1.dialogueFont is null) {
				disableValidation = true;
			}
			else if (texture == Game1.dialogueFont.Texture) {
				return true;
			}

			return false;
		}

		var isText = texture.Format == SurfaceFormat.Dxt3;
		var isLargeText = isText && IsLargeFont(texture);
		var isSmallText = isText && !isLargeText;

		if (isLargeText) {
			meta.Flags |= Texture2DMeta.TextureFlag.IsLargeFont;
		}
		else {
			meta.Flags &= ~Texture2DMeta.TextureFlag.IsLargeFont;
		}

		if (isSmallText) {
			meta.Flags |= Texture2DMeta.TextureFlag.IsSmallFont;
		}
		else {
			meta.Flags &= ~Texture2DMeta.TextureFlag.IsSmallFont;
		}

		if (!(Configuration.Preview.Override.Instance?.ResampleLargeText ?? Config.Resample.EnabledLargeText) && isLargeText) {
			if (!meta.TracePrinted) {
				meta.TracePrinted = true;
				Debug.Trace($"Not Scaling Texture '{texture.NormalizedName(DrawingColor.LightYellow)}', Is Font (and text resampling is disabled)");
			}
			meta.Validation = false;
			if (clean) {
				PurgeInvalidated(texture);
			}
			return false;
		}
		if (!(Configuration.Preview.Override.Instance?.ResampleSmallText ?? Config.Resample.EnabledSmallText) && isSmallText) {
			// The only BC2 texture that I've _ever_ seen is the internal font

			if (!meta.TracePrinted) {
				meta.TracePrinted = true;
				Debug.Trace($"Not Scaling Texture '{texture.NormalizedName(DrawingColor.LightYellow)}', Is Basic Font (and basic text resampling is disabled)");
			}
			meta.Validation = false;
			if (clean) {
				PurgeInvalidated(texture);
			}
			return false;
		}

		if (!texture.Anonymous()) {
			foreach (var blacklistPattern in Config.Resample.BlacklistPatterns) {
				if (!blacklistPattern.IsMatch(texture.NormalizedName())) {
					continue;
				}

				if (!meta.TracePrinted) {
					meta.TracePrinted = true;
					Debug.Trace($"Not Scaling Texture '{texture.NormalizedName(DrawingColor.LightYellow)}', Is Blacklisted ({blacklistPattern})");
				}
				meta.Validation = false;
				if (clean) {
					PurgeInvalidated(texture);
				}
				return false;
			}
		}

		if (!(Configuration.Preview.Override.Instance?.ResampleSprites ?? Config.Resample.EnabledSprites) && !isLargeText && !isSmallText) {
			if (!meta.TracePrinted) {
				meta.TracePrinted = true;
				Debug.Trace(
					$"Not Scaling Texture '{texture.NormalizedName(DrawingColor.LightYellow)}', Is Sprite (and sprite resampling is disabled)"
				);
			}

			meta.Validation = false;
			if (clean) {
				PurgeInvalidated(texture);
			}

			return false;
		}

		if (!disableValidation && (isText || !texture.Anonymous())) {
			meta.Validation = true;
		}

		return true;
	}

	private static readonly TexelTimer TexelAverage = new();
	private static readonly TexelTimer TexelAverageCached = new();
	private static readonly TexelTimer TexelAverageSync = new();
	private static readonly TexelTimer TexelAverageCachedSync = new();

	internal static void ClearTimers() {
		TexelAverage.Reset();
		TexelAverageCached.Reset();
		TexelAverageSync.Reset();
		TexelAverageCachedSync.Reset();
	}

	[MethodImpl(Runtime.MethodImpl.Inline)]
	private static TexelTimer GetTimer(bool cached, bool async) {
		if (async) {
			return cached ? TexelAverageCached : TexelAverage;
		}
		else {
			return cached ? TexelAverageCachedSync : TexelAverageSync;
		}
	}

	[MethodImpl(Runtime.MethodImpl.Inline)]
	private static TexelTimer GetTimer(XTexture2D texture, bool async, out bool isCached) {
		var isTextureCached = SpriteInfo.IsCached(texture);
		isCached = isTextureCached;
		return GetTimer(isTextureCached, async);
	}

#if DEBUG
	private static TimeSpan MeanTimeSpan = TimeSpan.Zero;
	private static int TimeSpanSamples = 0;
#endif

	internal static ManagedSpriteInstance? Fetch(XTexture2D texture, Bounds source, uint expectedScale) {
		if (!Config.IsEnabled || !Config.Resample.IsEnabled) {
			return null;
		}

		if (!Validate(texture, clean: true)) {
			return null;
		}

		if (SpriteMap.TryGetReady(texture, source, expectedScale, out var scaleTexture)) {
			if (scaleTexture.NoResample) {
				return null;
			}

			return scaleTexture;
		}

		return null;
	}

	internal static bool TryResurrect(in SpriteInfo.Initializer initializer, [NotNullWhen(true)] out ManagedSpriteInstance? resurrected) {
		// Check for a suspended sprite instance that happens to match.
		if (!Config.SuspendedCache.Enabled) {
			resurrected = null;
			return false;
		}

		var spriteHash = GetHash(initializer, initializer.TextureType);
		if (spriteHash.HasValue && SuspendedSpriteCache.TryFetch(spriteHash.Value, out var instance)) {
			var spriteMapHash = SpriteMap.SpriteHash(initializer.Reference, initializer.Bounds, initializer.ExpectedScale, initializer.IsPreview);
			if (instance.Resurrect(initializer.Reference, spriteMapHash)) {
				resurrected = instance;
				return true;
			}
		}

		resurrected = null;
		return false;
	}

	internal static bool TryResurrect(SpriteInfo info, [NotNullWhen(true)] out ManagedSpriteInstance? resurrected) {
		// Check for a suspended sprite instance that happens to match.
		if (!Config.SuspendedCache.Enabled) {
			resurrected = null;
			return false;
		}

		var spriteHash = GetHash(info, info.TextureType);
		if (SuspendedSpriteCache.TryFetch(spriteHash, out var instance)) {
			var spriteMapHash = SpriteMap.SpriteHash(info.Reference, info.Bounds, info.ExpectedScale, info.IsPreview);
			if (instance.Resurrect(info.Reference, spriteMapHash)) {
				resurrected = instance;
				return true;
			}
		}

		resurrected = null;
		return false;
	}

	private static readonly ThreadLocal<Stopwatch> FetchStopwatch = new(() => new());

	internal static ManagedSpriteInstance? FetchOrCreate(XTexture2D texture, Bounds source, uint expectedScale, bool sliced, out bool allowCache) {
		allowCache = true;

		if (!Config.IsEnabled || !Config.Resample.IsEnabled) {
			return null;
		}

		if (!Validate(texture, clean: true)) {
			return null;
		}

		var textureMeta = texture.Meta();

		// Is currentInstance referencing a texture-chain reference?
		bool textureChain = false;
		ManagedSpriteInstance? currentInstance = null;

		static bool ValidateInstance(ManagedSpriteInstance instance) {
			return
				!instance.IsDisposed &&
				instance.IsPreview == (Configuration.Preview.Override.Instance is not null);
		}

		if (SpriteMap.TryGet(texture, source, expectedScale, out var scaleTexture) && ValidateInstance(scaleTexture)) {
			if (scaleTexture.Invalidated) {
				currentInstance = scaleTexture;
				textureChain = true;
			}
			else switch (scaleTexture.IsReady)
			{
				case true:
					return scaleTexture;
				case false when scaleTexture.PreviousSpriteInstance is not null && scaleTexture.PreviousSpriteInstance.IsReady && ValidateInstance(scaleTexture.PreviousSpriteInstance):
					currentInstance = scaleTexture.PreviousSpriteInstance;
					textureChain = false;
					break;
				default:
					currentInstance = scaleTexture;
					textureChain = false;
					break;
			}
		}

		bool IsNoResample(ManagedSpriteInstance? instance) {
			if (textureChain || instance is null) {
				return false;
			}

			return instance.NoResample;
		}

		if (IsNoResample(currentInstance) || textureMeta.IsNoResample(source)) {
			return null;
		}

		bool useStalling = Config.Resample.UseFrametimeStalling && !GameState.IsLoading;

		bool useAsync =
			Config.AsyncScaling.Enabled &&
			(Config.AsyncScaling.EnabledForUnknownTextures || !texture.Anonymous()) &&
			source.Area >= Config.AsyncScaling.MinimumSizeTexels;
		// !textureMeta.HasCachedData

		TimeSpan? remainingTime = null;
		bool? isCached = null;

		string GetMetadataString() {
			return isCached.HasValue ?
				$" ({(useAsync ? "async" : "sync".Pastel(DrawingColor.Orange))} {(isCached.Value ? "cached" : "uncached".Pastel(DrawingColor.Orange))})" :
				$" ({(useAsync ? "async" : "sync".Pastel(DrawingColor.Orange))})";
		}

		string GetNameString() {
			return $"'{texture.NormalizedName(DrawingColor.LightYellow)}'{GetMetadataString()}";
		}

		// TODO : We should really only populate the average when we are performing an expensive operation like GetData.
		var watch = FetchStopwatch.Value!;
		watch.Restart();

		var textureType = (sliced, source != texture.Bounds()) switch {
			(true, _) => TextureType.SlicedImage,
			(false, true) => TextureType.Sprite,
			_ => TextureType.Image
		};

		// TODO : break this up somewhat so that we can delay hashing for things by _one_ frame (still deterministic, but offset so we can parallelize the work).
		// Presently, this cannot be done because the initializer is a 'ref struct' and is used immediately. If we want to check the suspended cache, it needs to be jammed away
		// so the hashing can be performed before the next frame.
		SpriteInfo.Initializer spriteInfoInitializer = new(
			reference: texture,
			dimensions: source,
			expectedScale: expectedScale,
			textureType: textureType
		);

		void RestorePriority(ManagedSpriteInstance? instance) {
			if (instance is not null && !instance.IsReady && instance.DeferredTask.TryGetTarget(out var task)) {
				if (task.IsPriorityDowngraded) {
					SynchronizedTaskScheduler.Instance.RestorePriority(task);
				}
				else {
					SynchronizedTaskScheduler.Instance.Bump(task);
				}
			}
		}

		// If the currentInstance hash matches, assume it's the same and just set it as ours.
		if (currentInstance is not null && currentInstance.SpriteInfoHash == spriteInfoInitializer.Hash) {
			currentInstance.Invalidated = false;
			SpriteMap.AddReplace(texture, currentInstance);

			RestorePriority(currentInstance);
			return currentInstance;
		}

		// Check for a suspended sprite instance that happens to match.
		if (TryResurrect(in spriteInfoInitializer, out var resurrectedInstance)) {
			if (currentInstance is not null && currentInstance != resurrectedInstance) {
				currentInstance.Suspend();
			}
			RestorePriority(resurrectedInstance);
			return resurrectedInstance;
		}

		// If there was a previous instance, return that for now.
		if (!textureChain && currentInstance is not null) {
			// It can be a previous sprite instance instead, so don't cache.
			allowCache = false;
			return currentInstance;
		}

		if (useStalling && DrawState.PushedUpdateWithin(0)) {
			remainingTime = DrawState.RemainingFrameTime();
			if (remainingTime <= TimeSpan.Zero) {
				allowCache = false;
				return currentInstance;
			}

			var estimatedDuration = GetTimer(texture: texture, async: useAsync, out bool cached).Estimate(texture.Format.SizeBytes(source.Area));
			isCached = cached;
			if (estimatedDuration > TimeSpan.Zero && estimatedDuration > remainingTime) {
				Debug.Trace($"Not enough frame time left to begin resampling {GetNameString()} ({estimatedDuration.TotalMilliseconds.ToString(DrawingColor.LightBlue)} ms >= {remainingTime?.TotalMilliseconds.ToString(DrawingColor.LightBlue)} ms)");
				allowCache = false;
				return currentInstance;
			}
		}

		string GetRemainingTime() {
			return remainingTime.HasValue
				? $" (remaining time: {remainingTime.Value.TotalMilliseconds.ToString(DrawingColor.LightYellow)} ms)" :
				"";
		}

		// If this is null, it can only happen due to something being blocked, so we should try again later.
		if (spriteInfoInitializer.ReferenceData is null) {
			Debug.Trace($"Texture Data fetch for {GetNameString()} was {"blocked".Pastel(DrawingColor.Red)}; retrying later{GetRemainingTime()}");
			allowCache = false;
			return currentInstance;
		}

		DrawState.IsUpdatedThisFrame = true;

		try {
			bool doDispatch = true;
			var currentRevision = textureMeta.Revision;

			// Check if there is already an in-flight task for this instance.
			// TODO : this logic feels duplicated - we can already query for the Instance, and it already holds a WeakReference to the task...
			if (textureMeta.InFlightTasks.TryGetValue(source, out var inFlightTask)) {
				doDispatch = inFlightTask.Revision != currentRevision || inFlightTask.ResampleTask.Status != TaskStatus.WaitingToRun;
			}

			Task<ManagedSpriteInstance> resampleTask;
			if (doDispatch) {
				resampleTask = ResampleTask.Dispatch(
					spriteInfo: new(spriteInfoInitializer),
					async: useAsync,
					previousInstance: currentInstance
				);
				textureMeta.InFlightTasks[source] = new(currentRevision, resampleTask);
			}
			else {
				return null;
			}

			var result = resampleTask.IsCompletedSuccessfully ? resampleTask.Result : currentInstance;

			if (useAsync) {
				// It adds itself to the relevant maps.
				return (result?.IsReady ?? false) ? result : null;
			}
			else {
				return result;
			}
		}
		finally {
			watch.Stop();
			var duration = watch.Elapsed;
			var averager = GetTimer(cached: spriteInfoInitializer.WasCached, async: useAsync);
#if DEBUG
			TimeSpanSamples++;
			MeanTimeSpan += duration;
			var remainingTimeStr = GetRemainingTime();
			if (!string.IsNullOrEmpty(remainingTimeStr)) {
				remainingTimeStr = $"({remainingTimeStr} was remaining)";
			}
			Debug.Trace($"Rescale Duration {GetNameString()}: {(MeanTimeSpan / TimeSpanSamples).TotalMilliseconds.ToString(DrawingColor.LightYellow)} ms {remainingTimeStr}");
#endif
			averager.Add(source.Area, duration);
		}
	}

	internal static readonly SurfaceFormat[] AllowedFormats = {
		SurfaceFormat.Color,
		SurfaceFormat.Dxt5,
		SurfaceFormat.Dxt5SRgb,
		SurfaceFormat.Dxt3, // fonts
		SurfaceFormat.Dxt3SRgb,
		SurfaceFormat.Dxt1,
		SurfaceFormat.Dxt1SRgb,
		SurfaceFormat.Dxt1a
	};

	internal ManagedTexture2D? Texture = null;
	internal readonly string Name;
	private Vector2F ScaleInternal;

	internal Vector2F ScaleReciprocal { get; private set; }
	internal Vector2F Scale {
		get => ScaleInternal;
		private set {
			ScaleInternal = value;
			ScaleReciprocal = Vector2F.One / value;
		}
	}

	internal readonly TextureType TexType;

	private volatile bool IsLoaded = false;
	internal bool IsReady => IsLoaded && Texture is not null;
	internal readonly WeakReference<SynchronizedTaskScheduler.TextureActionTask> DeferredTask = new(null!);

	internal readonly Vector2B Wrapped = Vector2B.False;

	internal readonly WeakTexture Reference;
	internal readonly Bounds OriginalSourceRectangle;
	internal readonly ulong SpriteInfoHash = 0U;
	internal readonly ulong Hash = 0U;

	internal PaddingQuad Padding = PaddingQuad.Zero;
	internal Vector2I UnpaddedSize;
	internal Vector2I BlockPadding = Vector2I.Zero;
	private readonly Vector2I OriginalSize;
	private readonly Bounds SourceRectangle;
	internal IScalerInfo? ScalerInfo = null;
	internal ulong SpriteMapHash { get; private set; }
	private readonly uint ReferenceScale;
	internal ManagedSpriteInstance? PreviousSpriteInstance = null;
	internal volatile bool Invalidated = false;
	internal readonly InterlockedBool Suspended = false;
	internal bool NoResample = false;
	internal readonly bool IsPreview = false;

	internal ulong LastReferencedFrame = DrawState.CurrentFrame;

	internal Vector2F InnerRatio = Vector2F.One;

	/// <summary>
	/// Node into the most-recent accessed instance list.
	/// Should only be <seealso langword="null"/> after the instance is <seealso cref="ManagedSpriteInstance.Dispose">disposed</seealso>
	/// </summary>
	internal ConcurrentLinkedListSlim<WeakInstance>.NodeRef RecentAccessNode = default;

	internal readonly InterlockedBool IsDisposed = false;

	internal static long TotalMemoryUsage = 0U;

	internal long MemorySize {
		[MethodImpl(Runtime.MethodImpl.Inline)]
		get {
			if (!IsReady || Texture is null) {
				return 0;
			}
			return Texture.SizeBytes();
		}
	}

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static void Purge(XTexture2D reference, Bounds? bounds, in DataRef<byte> data, bool animated = false) {
		SpriteInfo.Purge(reference, bounds, data, animated: animated);
		if (data.IsNull) {
			SpriteMap.Purge(reference, bounds, animated: animated);
		}
		else {
			SpriteMap.Invalidate(reference, bounds, animated: animated);
		}
		//Resampler.PurgeHash(reference);
	}

	[MethodImpl(Runtime.MethodImpl.Inline)]
	internal static void FullPurge(XTexture2D reference, bool animated = false) {
		SpriteInfo.Purge(reference, reference.Bounds, DataRef<byte>.Null, animated: animated);
		SpriteMap.Purge(reference, reference.Bounds, animated: animated);
	}

	internal static void PurgeTextures(long purgeTotalBytes) {
		purgeTotalBytes.AssertPositive();

		Debug.Trace($"Attempting to purge {purgeTotalBytes.AsDataSize()} from currently loaded textures");

		lock (RecentAccessList) {
			long totalPurge = 0;
			while (purgeTotalBytes > 0 && RecentAccessList.TryRemoveLast(out var lastInstance)) {
				if (lastInstance.TryGet(out var target)) {
					var textureSize = target.MemorySize;
					Debug.Trace($"Purging {target.NormalizedName()} ({textureSize.AsDataSize()})");
					purgeTotalBytes -= textureSize;
					totalPurge += textureSize;
					target.RecentAccessNode = default;
					target.Dispose(true);
				}
			}
			Debug.Trace($"Total Purged: {totalPurge.AsDataSize()}");
		}
	}

	internal ManagedSpriteInstance(string assetName, SpriteInfo spriteInfo, Bounds sourceRectangle, TextureType textureType, bool async, uint expectedScale, ManagedSpriteInstance? previous = null) {
		PreviousSpriteInstance = previous;

		TexType = textureType;

		var source = spriteInfo.Reference;

		OriginalSourceRectangle = sourceRectangle;
		Reference = source.MakeWeak();
		SourceRectangle = sourceRectangle;
		ReferenceScale = expectedScale;
		IsPreview = spriteInfo.IsPreview;
		SpriteMapHash = SpriteMap.SpriteHash(source, sourceRectangle, expectedScale, spriteInfo.IsPreview);
		Name = source.Anonymous() ? assetName.NormalizedName() : source.NormalizedName();
		// TODO : I believe we need a lock here until when the texture is _fully created_, preventing new instantiations from starting of a texture
		// already in-flight
		if (!SpriteMap.AddReplaceInvalidated(source, this)) {
			// If false, then the sprite already exists in the map (which can be caused by gap between the Resample task being kicked off, and hitting this, and _another_ sprite getting
			// past the earlier try-block, and getting here.
			// TODO : this should be fixed by making sure that all of the resample tasks _at least_ get to this point before the end of the frame.
			// TODO : That might not be sufficient either if the _same_ draw ends up happening again.
			Dispose();

			return;
		}

		OriginalSize = TexType switch {
			TextureType.Sprite => sourceRectangle.Extent,
			TextureType.Image => source.Extent(),
			TextureType.SlicedImage => sourceRectangle.Extent,
			_ => OriginalSize
		};

		// TODO store the HD Texture in _this_ object instead. Will confuse things like subtexture updates, though.
		SpriteInfoHash = spriteInfo.Hash;
		Hash = GetHash(spriteInfo, textureType);
		Texture = Resampler.Upscale(
			spriteInstance: this,
			scale: ref ReferenceScale,
			input: spriteInfo,
			hash: Hash,
			wrapped: ref Wrapped,
			async: false
		);

		// TODO : I would love to dispose of this texture _now_, but we rely on it disposing to know if we need to dispose of ours.
		// There is a better way to do this using weak references, I just need to analyze it further. Memory leaks have been a pain so far.
		// TODO 2: This won't get hit if the texture is disposed of via the finalizer.
		source.Disposing += OnParentDispose;

		lock (RecentAccessList) {
			RecentAccessNode = RecentAccessList.AddFront(this.MakeWeak());
		}
	}

	[MethodImpl(Runtime.MethodImpl.Inline)]
	public void OnParentDispose(object? sender, EventArgs args) => OnParentDispose(sender as XTexture2D);

	// Async Call
	internal void Finish() {
		ManagedTexture2D? texture;
		lock (this) {
			texture = Texture;
		}

		if (texture is null || texture.IsDisposed || IsDisposed) {
			return;
		}

		UpdateReferenceFrame();

		Interlocked.Add(ref TotalMemoryUsage, texture.SizeBytes());
		// TODO : this won't get hit if the texture is disposed of via the finalizer
		texture.Disposing += (_, _) => { Interlocked.Add(ref TotalMemoryUsage, -texture.SizeBytes()); };

		{
#if DEBUG
			var formatString = texture.Format.ToString(DrawingColor.LightCoral);
			var nameString = this.NormalizedName(DrawingColor.LightYellow);
			switch (TexType) {
				case TextureType.Sprite:
					Debug.Trace($"Creating Sprite [{formatString} x{ReferenceScale}]: {nameString} {SourceRectangle}");
					break;
				case TextureType.Image:
					Debug.Trace($"Creating Image [{formatString} x{ReferenceScale}]: {nameString}");
					break;
				case TextureType.SlicedImage:
					Debug.Trace($"Creating Sliced Image [{formatString} x{ReferenceScale}]: {nameString}");
					break;
				default:
					Debug.Trace($"Creating UNKNOWN [{formatString} x{ReferenceScale}]: {nameString}");
					break;
			}
#endif
		}

		Scale = (Vector2F)texture.Dimensions / (Vector2F)OriginalSize;

		Thread.MemoryBarrier();
		IsLoaded = true;
		if (PreviousSpriteInstance is not null) {
			PreviousSpriteInstance.Suspend(true);
			PreviousSpriteInstance = null;
		}
	}

	internal void UpdateReferenceFrame() {
		if (IsDisposed) {
			return;
		}

		LastReferencedFrame = DrawState.CurrentFrame;

		if (RecentAccessNode.IsValid) {
			RecentAccessList.MoveToFront(RecentAccessNode);
		}
		else {
			RecentAccessNode = RecentAccessList.AddFront(this.MakeWeak());
		}
	}

	[StructLayout(LayoutKind.Auto)]
	internal readonly struct CleanupData {
		internal readonly ManagedSpriteInstance? PreviousSpriteInstance;
		internal readonly WeakReference<XTexture2D> ReferenceTexture;
		internal readonly ConcurrentLinkedListSlim<WeakInstance>.NodeRef RecentAccessNode;
		internal readonly ulong MapHash;

		internal CleanupData(ManagedSpriteInstance instance) {
			PreviousSpriteInstance = instance.PreviousSpriteInstance;
			ReferenceTexture = instance.Reference;
			RecentAccessNode = instance.RecentAccessNode;
			instance.RecentAccessNode = default;
			MapHash = instance.SpriteMapHash;
		}
	}

	[MethodImpl(Runtime.MethodImpl.Inline)]
	~ManagedSpriteInstance() {
		if (IsDisposed) {
			return;
		}
		//Debug.Trace($"ManagedSpriteInstance '{Name}' reached destructor; was not initially disposed");
		ReleasedSpriteInstances.Push(new(this));
	}

	internal void Dispose(bool disposeChildren) {
		if (disposeChildren && Texture is not null) {
			if (!Texture.IsDisposed) {
				Texture.Dispose();
			}
			Texture = null;
		}
		Dispose();
	}

	internal void DisposeSuspended() {
		if (Suspended.CompareExchange(false, true) == true) {
			Dispose();
		}
	}

	public void Dispose() {
		if (IsDisposed.CompareExchange(true, false) == true) {
			return;
		}

		if (PreviousSpriteInstance is not null) {
			PreviousSpriteInstance.Suspend(true);
			PreviousSpriteInstance = null;
		}

		if (Reference.TryGetTarget(out var reference)) {
			SpriteMap.Remove(this, reference);
			reference.Disposing -= OnParentDispose;
		}

		if (RecentAccessNode.IsValid) {
			RecentAccessList.Release(ref RecentAccessNode);
			RecentAccessNode = default;
		}

		if (Suspended.CompareExchange(false, true) == true) {
			SuspendedSpriteCache.RemoveFast(this);
		}

		GC.SuppressFinalize(this);
	}

	internal static void Cleanup(in CleanupData data) {
		Debug.Trace("Cleaning up finalized ManagedSpriteInstance");
		if (data.PreviousSpriteInstance is not null) {
			data.PreviousSpriteInstance.Suspend(true);
		}

		if (data.ReferenceTexture.TryGetTarget(out var reference)) {
			SpriteMap.Remove(data, reference);
		}

		if (data.RecentAccessNode.IsValid) {
			RecentAccessList.Release(ref Unsafe.AsRef(data.RecentAccessNode));
		}
	}

	[MethodImpl(Runtime.MethodImpl.Inline)]
	private void OnParentDispose(XTexture2D? texture) {
		Debug.Trace($"Parent Texture Disposing: {texture?.NormalizedName() ?? "[NULL]"}, suspending/disposing ManagedSpriteInstance");

		Suspend();
	}

	internal void Suspend(bool clearChildrenIfDispose = false) {
		if (IsDisposed) {
			return;
		}

		if (Suspended.CompareExchange(true, false) == true) {
			return;
		}

		if (StardewValley.Game1.quit) {
			return;
		}

		if (!IsLoaded || !Config.SuspendedCache.Enabled) {
			Dispose(clearChildrenIfDispose);
			return;
		}

		// TODO : Handle clearing any reference to _this_
		PreviousSpriteInstance = null;

		if (RecentAccessNode.IsValid) {
			RecentAccessList.Release(ref RecentAccessNode);
			RecentAccessNode = default;
		}

		Invalidated = false;

		Reference.SetTarget(null!);

		SuspendedSpriteCache.Add(this);

		Debug.Trace($"Sprite Instance '{Name}' {"Suspended".Pastel(DrawingColor.LightGoldenrodYellow)}");
	}

	internal bool Resurrect(XTexture2D texture, ulong spriteMapHash) {
		if (StardewValley.Game1.quit) {
			return false;
		}

		if (IsDisposed || Suspended.CompareExchange(false, true) != true) {
			SuspendedSpriteCache.RemoveFast(this);
			return false;
		}

		if (!IsLoaded || !Config.SuspendedCache.Enabled) {
			SuspendedSpriteCache.RemoveFast(this);
			return false;
		}

		SuspendedSpriteCache.Resurrect(this);
		Reference.SetTarget(texture);

		SpriteMapHash = spriteMapHash;
		texture.Meta().ReplaceInSpriteInstanceTable(SpriteMapHash, this);
		SpriteMap.AddReplace(texture, this);

		Debug.Trace($"Sprite Instance '{Name}' {"Resurrected".Pastel(DrawingColor.LightCyan)}");

		return true;
	}

	public long SizeBytes => (int)MemorySize;
}
