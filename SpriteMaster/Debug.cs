﻿using Microsoft.Xna.Framework.Graphics;
using SpriteMaster.Attributes;
using SpriteMaster.Extensions;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SpriteMaster {
	internal static class Debug {
		private static readonly string ModuleName = typeof(Debug).Namespace;

		private const bool AlwaysFlush = false;

		private static class Color {
			internal const ConsoleColor Trace = ConsoleColor.Gray;
			internal const ConsoleColor Info = ConsoleColor.White;
			internal const ConsoleColor Warning = ConsoleColor.Yellow;
			internal const ConsoleColor Error = ConsoleColor.Red;
		}

		private static readonly string LocalLogPath = Path.Combine(Config.LocalRoot, $"{ModuleName}.log");
		private static readonly StreamWriter LogFile = null;

		static Debug () {
			if (Config.Debug.Logging.OwnLogFile) {
				// For some reason, on Linux it breaks if the log file could not be created?
				try {
					Directory.CreateDirectory(Path.GetDirectoryName(LocalLogPath));
					LogFile = new StreamWriter(
						path: LocalLogPath,
						append: false
					);
				}
				catch {
					WarningLn($"Could not create log file at {LocalLogPath}");
				}
			}
		}

		// Logging Stuff

		[DebuggerStepThrough, DebuggerHidden(), Untraced]
		static private string Format(this string memberName, bool format = true) {
			return (!format || memberName == null) ? "" : $"[{memberName}] ";
		}

		[Conditional("DEBUG"), DebuggerStepThrough, DebuggerHidden(), Untraced]
		static internal void Trace (string message, bool format = true, [CallerMemberName] string caller = null) {
			if (!Config.Debug.Logging.LogInfo)
				return;
			Console.Error.DebugWriteStr($"{caller.Format(format)}{message}", LogLevel.Debug);
		}

		[Conditional("DEBUG"), DebuggerStepThrough, DebuggerHidden(), Untraced]
		static internal void Trace<T> (T exception, [CallerMemberName] string caller = null) where T : Exception {
			if (!Config.Debug.Logging.LogInfo)
				return;
			TraceLn($"Exception: {exception.Message}", caller: caller);
			TraceLn(exception.GetStackTrace().Replace(" at", "\n\tat"), caller: caller);
		}

		[Conditional("DEBUG"), DebuggerStepThrough, DebuggerHidden(), Untraced]
		static internal void TraceLn (string message, bool format = true, [CallerMemberName] string caller = null) {
			Trace($"{message}\n", format, caller);
		}

		[Conditional("TRACE"), DebuggerStepThrough, DebuggerHidden(), Untraced]
		static internal void Info (string message, bool format = true, [CallerMemberName] string caller = null) {
			if (!Config.Debug.Logging.LogInfo)
				return;
			Console.Error.DebugWriteStr($"{caller.Format(format)}{message}", LogLevel.Info);
		}

		[Conditional("TRACE"), DebuggerStepThrough, DebuggerHidden(), Untraced]
		static internal void Info<T>(T exception, [CallerMemberName] string caller = null) where T : Exception {
			if (!Config.Debug.Logging.LogInfo)
				return;
			InfoLn($"Exception: {exception.Message}", caller: caller);
			InfoLn(exception.GetStackTrace().Replace(" at", "\n\tat"), caller: caller);
		}

		[Conditional("TRACE"), DebuggerStepThrough, DebuggerHidden(), Untraced]
		static internal void InfoLn (string message, bool format = true, [CallerMemberName] string caller = null) {
			Info($"{message}\n", format, caller);
		}

		[DebuggerStepThrough, DebuggerHidden(), Untraced]
		static internal void Warning (string message, bool format = true, [CallerMemberName] string caller = null) {
			if (!Config.Debug.Logging.LogWarnings)
				return;
			Console.Error.DebugWrite(LogLevel.Warn, $"{caller.Format(format)}{message}");
		}

		[DebuggerStepThrough, DebuggerHidden(), Untraced]
		static internal void Warning<T> (T exception, [CallerMemberName] string caller = null) where T : Exception {
			if (!Config.Debug.Logging.LogInfo)
				return;
			WarningLn($"Exception: {exception.Message}", caller: caller);
			WarningLn(exception.GetStackTrace().Replace(" at", "\n\tat"), caller: caller);
		}

		[DebuggerStepThrough, DebuggerHidden(), Untraced]
		static internal void WarningLn (string message, bool format = true, [CallerMemberName] string caller = null) {
			Warning($"{message}\n", format, caller);
		}

		[DebuggerStepThrough, DebuggerHidden(), Untraced]
		static internal void Error (string message, bool format = true, [CallerMemberName] string caller = null) {
			if (!Config.Debug.Logging.LogErrors)
				return;
			Console.Error.DebugWrite(LogLevel.Error, $"{caller.Format(format)}{message}");
		}

		[DebuggerStepThrough, DebuggerHidden(), Untraced]
		static internal void Error<T> (T exception, [CallerMemberName] string caller = null) where T : Exception {
			if (!Config.Debug.Logging.LogInfo)
				return;
			ErrorLn($"Exception: {exception.Message}", caller: caller);
			ErrorLn(exception.GetStackTrace().Replace(" at", "\n\tat"), caller: caller);
		}

		[DebuggerStepThrough, DebuggerHidden(), Untraced]
		static internal void ErrorLn (string message, bool format = true, [CallerMemberName] string caller = null) {
			Error($"{message}\n", format, caller);
		}

		[DebuggerStepThrough, DebuggerHidden(), Untraced]
		static internal void Flush () {
			Console.Error.FlushAsync();
		}

		static internal void DumpMemory () {
			lock (Console.Error) {
				var duplicates = new Dictionary<string, List<Texture2D>>();
				bool haveDuplicates = false;

				var textureDump = ScaledTexture.SpriteMap.GetDump();
				long totalSize = 0;
				long totalOriginalSize = 0;
				ErrorLn("Texture Dump:");
				foreach (var list in textureDump) {
					var referenceTexture = list.Key;
					long originalSize = (referenceTexture.Area() * sizeof(int));
					bool referenceDisposed = referenceTexture.IsDisposed;
					totalOriginalSize += referenceDisposed ? 0 : originalSize;
					ErrorLn($"SpriteSheet: {referenceTexture.SafeName().Enquote()} :: Original Size: {originalSize.AsDataSize()}{(referenceDisposed ? " [DISPOSED]" : "")}");

					if (!referenceTexture.Name.IsBlank() && !referenceTexture.IsDisposed) {
						List<Texture2D> duplicateList;
						if (!duplicates.TryGetValue(referenceTexture.Name, out duplicateList)) {
							duplicateList = new List<Texture2D>();
							duplicates.Add(referenceTexture.Name, duplicateList);
						}
						duplicateList.Add(referenceTexture);
						haveDuplicates = haveDuplicates || (duplicateList.Count > 1);
					}

					foreach (var sprite in list.Value) {
						if (sprite.IsReady && sprite.Texture != null) {
							var spriteDisposed = sprite.Texture.IsDisposed;
							ErrorLn($"\tSprite: {sprite.OriginalSourceRectangle} :: {sprite.MemorySize.AsDataSize()}{(spriteDisposed ? " [DISPOSED]" : "")}");
							totalSize += spriteDisposed ? 0 : sprite.MemorySize;
						}
					}
				}
				ErrorLn($"Total Resampled Size: {totalSize.AsDataSize()}");
				ErrorLn($"Total Original Size: {totalOriginalSize.AsDataSize()}");
				ErrorLn($"Total Size: {(totalOriginalSize + totalSize).AsDataSize()}");

				if (haveDuplicates) {
					ErrorLn("Duplicates:");
					foreach (var duplicate in duplicates) {
						long size = 0;
						foreach (var subDuplicate in duplicate.Value) {
							size += subDuplicate.Area() * sizeof(int);
						}

						ErrorLn($"\t{duplicate.Key.Enquote()} :: {duplicate.Value.Count.Delimit()} duplicates :: Total Size: {size.AsDataSize()}");
					}
				}

				Console.Error.Flush();
			}
		}

		[DebuggerStepThrough, DebuggerHidden(), Untraced]
		static private ConsoleColor GetColor(this LogLevel @this) {
			return @this switch {
				LogLevel.Debug => Color.Trace,
				LogLevel.Info => Color.Info,
				LogLevel.Warn => Color.Warning,
				LogLevel.Error => Color.Error,
				_ => ConsoleColor.White,
			};
		}

		[DebuggerStepThrough, DebuggerHidden(), Untraced]
		static private void DebugWrite (this TextWriter writer, LogLevel level, string str) {
			lock (writer) {
				if (LogFile != null) {
					try {
						var prefix = level switch{
							LogLevel.Debug => 'T',
							LogLevel.Info => 'I',
							LogLevel.Warn => 'W',
							LogLevel.Error => 'E',
							_ => '?',
						};

						LogFile.Write($"[{prefix}] {str}");
					}
					catch { /* ignore errors */ }
				}

				var originalColor = Console.ForegroundColor;
				Console.ForegroundColor = level.GetColor();
				try {
					writer.DebugWriteStr(str, level);
				}
				finally {
					Console.ForegroundColor = originalColor;
				}
			}
		}

		[DebuggerStepThrough, DebuggerHidden(), Untraced]
		static private void DebugWriteStr (this TextWriter writer, string str, LogLevel level) {
			if (Config.Debug.Logging.UseSMAPI) {
				var strings = str.Split(new[] { '\n', '\r' });
				if (strings[strings.Length - 1] == "") {
					strings[strings.Length - 1] = null;
				}
				foreach (var line in strings) {
					if (line == null)
						continue;
					SpriteMaster.Self.Monitor.Log(line.TrimEnd(), level);
				}
			}
			else {
				var strings = str.Split('\n');
				foreach (var line in strings) {
					if (line == "") {
						writer.Write("\n");
					}
					else {
						writer.Write($"[{ModuleName}] {line}");
					}
				}
				if (AlwaysFlush)
					writer.FlushAsync();
			}
		}
	}
}
