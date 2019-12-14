﻿using LinqFasterer;
using SpriteMaster.Caching;
using SpriteMaster.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SpriteMaster;

internal partial class Debug {
	internal static void DumpAllStats() {
		var currentProcess = Process.GetCurrentProcess();
		var workingSet = currentProcess.WorkingSet64;
		var virtualMem = currentProcess.VirtualMemorySize64;
		var gcAllocated = GC.GetTotalMemory(false);

		var lines = new List<string> {
			"SpriteMaster Stats Dump:",
			"\tVM:",
			$"\t\tProcess Working Set    : {workingSet.AsDataSize()}",
			$"\t\tProcess Virtual Memory : {virtualMem.AsDataSize()}:",
			$"\t\tGC Allocated Memory    : {gcAllocated.AsDataSize()}:",
			"",
			"\tSuspended Sprite Cache Stats:"
		};

		lines.AddRange(SuspendedSpriteCache.DumpStats().SelectF(s => $"\t{s}"));
		lines.Add("");

		ManagedTexture2D.DumpStats(lines);

		foreach (var line in lines) {
			Message(line);
		}

		Message("");

		Message($"TextureFileCache: {TextureFileCache.Size.AsDataSize()}");
		Message($"ResidentCache: {ResidentCache.Size.AsDataSize()}");
		Message($"SuspendedSpriteCache: {SuspendedSpriteCache.Size.AsDataSize()}");
	}
}
