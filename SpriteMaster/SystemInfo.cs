﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteMaster.Extensions;
using System;
using System.Text;

namespace SpriteMaster;

static class SystemInfo {
	internal static void Dump(GraphicsDeviceManager gdm, GraphicsDevice device) {
		var dumpBuilder = new StringBuilder();

		dumpBuilder.AppendLine("System Information:");

		try {
			dumpBuilder.AppendLine($"\tArchitecture: {(Environment.Is64BitProcess ? "x64" : "x86")}");
			dumpBuilder.AppendLine($"\tNumber of Cores: {Environment.ProcessorCount}");
			dumpBuilder.AppendLine($"\tOS Version: {Environment.OSVersion}");
		}
		catch { }

		try {
			var memoryInfo = GC.GetGCMemoryInfo();
			dumpBuilder.AppendLine($"\tTotal Committed Memory: {memoryInfo.TotalCommittedBytes.AsDataSize()}");
			dumpBuilder.AppendLine($"\tTotal Available Memory: {memoryInfo.TotalAvailableMemoryBytes.AsDataSize()}");
		}
		catch { }

		try {
			if (!(device?.IsDisposed).GetValueOrDefault(false)) {
				var adapter = device?.Adapter;
				if (adapter != null) {
					dumpBuilder.AppendLine($"\tGraphics Adapter: {adapter}");
					dumpBuilder.AppendLine($"\tGraphics Adapter Description: {adapter.Description}");
				}
			}
		}
		catch { }

		Debug.Message(dumpBuilder.ToString());
	}
}
