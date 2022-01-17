﻿using Microsoft.Xna.Framework.Graphics;
using SpriteMaster.Caching;
using SpriteMaster.Extensions;
using SpriteMaster.Resample;
using SpriteMaster.Types;
using SpriteMaster.Types.Interlocking;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using SpriteDictionary = System.Collections.Generic.Dictionary<ulong, SpriteMaster.ManagedSpriteInstance>;

#nullable enable

namespace SpriteMaster.Metadata;

sealed class Texture2DMeta {
	internal readonly SpriteDictionary SpriteInstanceTable = new();

	internal void ClearSpriteInstanceTable() {
		foreach (var spriteInstance in SpriteInstanceTable.Values) {
			spriteInstance?.Dispose();
		}
		SpriteInstanceTable.Clear();
	}

	/// <summary>The current (static) ID, incremented every time a new <see cref="Texture2DMeta"/> is created.</summary>
	private static ulong CurrentID = 0U;
	/// <summary>Whenever a new <see cref="Texture2DMeta"/> is created, <see cref="CurrentID"/> is incremented and <see cref="UniqueIDString"/> is set to a string representation of it.</summary>
	private readonly string UniqueIDString = Interlocked.Increment(ref CurrentID).ToString64();

	internal readonly SharedLock Lock = new(LockRecursionPolicy.SupportsRecursion);

	internal volatile bool TracePrinted = false;

	internal bool? Validation = null;
	internal bool IsSystemRenderTarget = false;
	internal bool IsCompressed = false;
	internal ReportOnceErrors ReportedErrors = 0;
	internal SurfaceFormat Format;
	internal Vector2I Size;

	internal InterlockedULong LastAccessFrame { get; private set; } = (ulong)DrawState.CurrentFrame;
	internal InterlockedULong Hash { get; private set; } = 0;

	internal Texture2DMeta(Texture2D texture) {
		IsCompressed = texture.Format.IsCompressed();
		Format = texture.Format;
		Size = texture.Extent();
	}

	// TODO : this presently is not threadsafe.
	private readonly WeakReference<byte[]> _CachedData = (Config.MemoryCache.Enabled) ? new(null!) : null!;
	private readonly WeakReference<byte[]> _CachedRawData = (Config.MemoryCache.Enabled) ? new(null!) : null!;

	internal bool HasCachedData {
		[MethodImpl(Runtime.MethodImpl.Hot)]
		get {
			if (!Config.MemoryCache.Enabled) {
				return false;
			}

			using (Lock.Read) {
				return _CachedData.TryGetTarget(out var target);
			}
		}
	}

	[MethodImpl(Runtime.MethodImpl.Hot)]
	internal void Purge(Texture2D reference, in Bounds? bounds, in DataRef<byte> data) {
		using (Lock.Write) {
			bool hasCachedData = CachedRawData is not null;

			if (data.IsNull) {
				if (!hasCachedData) {
					Debug.TraceLn($"Clearing '{reference.SafeName(DrawingColor.LightYellow)}' Cache");
				}
				CachedRawData = null;
				return;
			}

			var refSize = (int)reference.SizeBytes();

			bool forcePurge = false;

			try {
				// TODO : lock isn't granular enough.
				if (Config.MemoryCache.AlwaysFlush) {
					forcePurge = true;
				}
				else if (!bounds.HasValue && data.Offset == 0 && data.Length == refSize) {
					Debug.TraceLn($"{(hasCachedData ? "Overriding" : "Setting")} '{reference.SafeName(DrawingColor.LightYellow)}' Cache in Purge: {bounds.HasValue}, {data.Offset}, {data.Length}");
					CachedRawData = data.Data;
				}
				// TODO : This doesn't update the compressed cache.
				else if (!IsCompressed && !bounds.HasValue && CachedRawData is var currentData && currentData is not null) {
					Debug.TraceLn($"{(hasCachedData ? "Updating" : "Setting")} '{reference.SafeName(DrawingColor.LightYellow)}' Cache in Purge: {bounds.HasValue}");
					var byteSpan = data.Data;
					var untilOffset = Math.Min(currentData.Length - data.Offset, data.Length);
					foreach (int i in 0.RangeTo(untilOffset)) {
						currentData[i + data.Offset] = byteSpan[i];
					}
					Hash = 0;
					CachedRawData = currentData; // Force it to update the global cache.
				}
				else {
					if (hasCachedData) {
						Debug.TraceLn($"Forcing full '{reference.SafeName(DrawingColor.LightYellow)}' Purge");
					}
					forcePurge = true;
				}
			}
			catch (Exception ex) {
				ex.PrintInfo();
				forcePurge = true;
			}

			// TODO : maybe we need to purge more often?
			if (forcePurge && hasCachedData) {
				CachedRawData = null;
			}
		}
	}

	internal static readonly byte[] BlockedSentinel = new byte[1] { 0xFF };

	internal byte[]? CachedDataNonBlocking {
		[MethodImpl(Runtime.MethodImpl.Hot)]
		get {
			if (!Config.MemoryCache.Enabled) {
				return null;
			}

			using (var locked = Lock.TryRead) if (locked) {
				_CachedRawData.TryGetTarget(out var target);
				return target;
			}
			return BlockedSentinel;
		}
	}

	internal byte[]? CachedRawData {
		[MethodImpl(Runtime.MethodImpl.Hot)]
		get {
			if (!Config.MemoryCache.Enabled) {
				return null;
			}

			using (Lock.Read) {
				_CachedRawData.TryGetTarget(out var target);
				return target;
			}
		}
		[MethodImpl(Runtime.MethodImpl.Hot)]
		set {
			try {
				if (!Config.MemoryCache.Enabled) {
					return;
				}

				TracePrinted = false;

				//if (_CachedRawData.TryGetTarget(out var target) && target == value) {
				//	return;
				//}
				if (value is null) {
					using (Lock.Promote) {
						_CachedRawData.SetTarget(null!);
						_CachedData.SetTarget(null!);
						ResidentCache.Remove<byte[]>(UniqueIDString);
					}
				}
				else {
					using (Lock.Promote) {
						ResidentCache.Set(UniqueIDString, value);
						_CachedRawData.SetTarget(value);
						if (!IsCompressed) {
							_CachedData.SetTarget(value);
						}
						else {
							_CachedData.SetTarget(null!);
							DecodeTask.Dispatch(this);
						}
					}
				}
			}
			finally {
				Hash = default;
			}
		}
	}

	internal byte[]? CachedData {
		get {
			if (_CachedData.TryGetTarget(out var data)) {
				return data;
			}
			return null;
		}
	}

	internal void SetCachedDataUnsafe(Span<byte> data) => _CachedData.SetTarget(data.ToArray());

	[MethodImpl(Runtime.MethodImpl.Hot)]
	internal void UpdateLastAccess() {
		LastAccessFrame = DrawState.CurrentFrame;
	}

	[MethodImpl(Runtime.MethodImpl.Hot)]
	internal ulong GetHash(SpriteInfo info) {
		using (Lock.Read) {
			ulong hash = Hash;
			if (hash == 0) {
				if (info.ReferenceData is null) {
					throw new NullReferenceException(nameof(info.ReferenceData));
				}
				hash = info.ReferenceData.Hash();
				using (Lock.Promote) {
					Hash = hash;
				}
			}
			return hash;
		}
	}

	[MethodImpl(Runtime.MethodImpl.Hot)]
	internal bool ShouldReportError(ReportOnceErrors error) {
		if ((ReportedErrors & error) != 0) {
			return false;
		}
		ReportedErrors |= error;
		return true;
	}
}