﻿using SpriteMaster.Metadata;
using SpriteMaster.Tasking;
using System;
using System.Threading.Tasks;

namespace SpriteMaster.Resample;

internal static class DecodeTask {
	private static readonly TaskFactory Factory = new(ThreadedTaskScheduler.Instance);

	private static void Decode(object? metadata) => DecodeFunction(metadata! as Texture2DMeta);

	private static void DecodeFunction(Texture2DMeta? metadata) {
		if (metadata is null) {
			return;
		}

		var rawData = metadata.CachedRawData;
		if (rawData is null) {
			return;
		}

		var uncompressedData = Resample.TextureDecode.DecodeBlockCompressedTexture(metadata.Format, metadata!.Size, rawData);
		if (uncompressedData.IsEmpty) {
			throw new InvalidOperationException("Compressed data failed to decompress");
		}
		metadata.SetCachedDataUnsafe(uncompressedData);
	}

	internal static Task Dispatch(Texture2DMeta metadata) => Factory.StartNew(Decode, metadata);
}
