﻿using SpriteMaster.Extensions;
using System.IO.Compression;

namespace Hashing.Benchmarks;

public class Dictionary : BenchmarkBaseImpl<DataSet<string[]>, string[]> {
	static Dictionary() {
		string[] words;
		var dictionary = Program.Options?.Dictionary ?? Options.Default.Dictionary;
		try {
			{
				using FileStream file = File.OpenRead(dictionary);
				if (Path.GetExtension(dictionary).EqualsInvariantInsensitive(".zip")) {
					using ZipArchive zip = new(file, ZipArchiveMode.Read, leaveOpen: false);
					using StreamReader reader = new(zip.Entries[0].Open());
					words = reader.ReadToEnd().Replace('\r', '\n').Split('\n', StringSplitOptions.RemoveEmptyEntries);
				}
				else {
					using StreamReader reader = new(file);
					words = reader.ReadToEnd().Replace('\r', '\n').Split('\n', StringSplitOptions.RemoveEmptyEntries);
				}
			}
		}
		catch (Exception ex) {
			throw new Exception($"Failed to open dictionary file '{dictionary}'");
		}

		void AddSet(in DataSet<string[]> dataSet) {
			DataSets.Add(dataSet);
		}

		AddSet(new(words));
	}
}