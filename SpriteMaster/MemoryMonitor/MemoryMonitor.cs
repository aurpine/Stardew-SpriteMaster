﻿using SpriteMaster.Caching;
using SpriteMaster.Configuration;
using SpriteMaster.Extensions;
using System;
using System.Runtime;
using System.Threading;

namespace SpriteMaster;

internal class MemoryMonitor {
	private readonly Thread MemoryPressureThread;
	private readonly Thread GarbageCollectThread;
	private readonly object CollectLock = new();

	internal MemoryMonitor() {
		MemoryPressureThread = new Thread(MemoryPressureLoop) {
			Name = "Memory Pressure Thread",
			Priority = ThreadPriority.BelowNormal,
			IsBackground = true
		};

		GarbageCollectThread = new Thread(GarbageCheckLoop) {
			Name = "Garbage Collection Thread",
			Priority = ThreadPriority.BelowNormal,
			IsBackground = true
		};
	}

	internal void Start() {
		if (MemoryPressureThread.ThreadState == ThreadState.Unstarted) {
			MemoryPressureThread.Start();
		}
		if (GarbageCollectThread.ThreadState == ThreadState.Unstarted) {
			GarbageCollectThread.Start();
		}
	}

	internal void TriggerGarbageCollection() {
		lock (CollectLock) {
			Garbage.Collect(compact: true, blocking: true, background: false);
			DrawState.TriggerCollection.Set(true);
		}
	}

	internal void TriggerPurge() {
		lock (CollectLock) {
			Garbage.Collect(compact: true, blocking: true, background: false);
			ResidentCache.Purge();
			Garbage.Collect(compact: true, blocking: true, background: false);
			DrawState.TriggerCollection.Set(true);
		}
	}

	private void MemoryPressureLoop() {
		for (; ; ) {
			if (DrawState.TriggerCollection && DrawState.TriggerCollection.Wait()) {
				continue;
			}

			lock (CollectLock) {
				try {
					using var _ = new MemoryFailPoint(Config.Garbage.RequiredFreeMemory);
				}
				catch (InsufficientMemoryException) {
					Debug.Warning($"Less than {(Config.Garbage.RequiredFreeMemory * 1024 * 1024).AsDataSize(decimals: 0)} available for block allocation, forcing full garbage collection");
					ResidentCache.Purge();
					SuspendedSpriteCache.Purge();
					DrawState.TriggerCollection.Set(true);
					Thread.Sleep(10000);
				}
			}
			Thread.Sleep(512);
		}
	}

	private void GarbageCheckLoop() {
		try {
			for (; ; ) {
				GC.RegisterForFullGCNotification(10, 10);
				GC.WaitForFullGCApproach();
				if (Garbage.ManualCollection) {
					Thread.Sleep(128);
					continue;
				}
				lock (CollectLock) {
					if (DrawState.TriggerCollection && DrawState.TriggerCollection.Wait()) {
						continue;
					}

					ResidentCache.Purge();
					DrawState.TriggerCollection.Set(true);
					// TODO : Do other cleanup attempts here.
				}
			}
		}
		catch {
			// ignored
		}
	}
}
