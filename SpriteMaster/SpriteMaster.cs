﻿using Harmony;
using SpriteMaster.Extensions;
using SpriteMaster.HarmonyExt;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.IO;
using System.Reflection;
using Tomlyn;
using Tomlyn.Syntax;

namespace SpriteMaster {
	public sealed class SpriteMaster : Mod {
		public static SpriteMaster Self { get => _Self; }
		private static SpriteMaster _Self = null;

		private static readonly string ConfigName = "config.toml";

		public SpriteMaster () {
			_Self = this;
		}

		public override void Entry (IModHelper help) {
			var ConfigPath = Path.Combine(help.DirectoryPath, ConfigName);
			LoadConfig(ConfigPath);
			SaveConfig(ConfigPath);
			
			ConfigureHarmony();
			help.Events.Input.ButtonPressed += OnButtonPressed;
		}

		private bool LoadConfig(string ConfigPath) {
			if (!File.Exists(ConfigPath)) {
				return false;
			}

			try {
				string ConfigText = File.ReadAllText(ConfigPath);
				var Data = Toml.Parse(ConfigText, ConfigPath);

				foreach (var table in Data.Tables) {
					var tableName = table.Name.ToString();
					var elements = tableName.Split('.');
					if (elements.Length != 0) {
						elements[0] = null;
					}
					var configClass = typeof(Config);
					string summedClass = configClass.Name.Trim();
					foreach (var element in elements) {
						if (element == null)
							continue;
						var trimmedElement = element.Trim();
						summedClass += $".{trimmedElement}";
						var child = configClass.GetNestedType(trimmedElement, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
						if (child == null || child.IsNestedPrivate || child.GetCustomAttribute<Config.FileIgnoreAttribute>() != null)
							throw new InvalidDataException($"Configuration Child Class '{summedClass}' does not exist");
						configClass = child;
					}

					foreach (var value in table.Items) {
						var keyString = value.Key.ToString().Trim();
						var field = configClass.GetField(keyString, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
						if (field == null || field.IsPrivate || field.IsInitOnly || !field.IsStatic || field.IsLiteral)
							throw new InvalidDataException($"Configuration Value '{summedClass}.{keyString}' does not exist");

						object fieldValue = field.GetValue(null);
						switch (fieldValue) {
							case string v:
								field.SetValue(null, (string)((StringValueSyntax)value.Value).Value.Trim());
								break;
							case sbyte v:
								field.SetValue(null, (sbyte)((IntegerValueSyntax)value.Value).Value);
								break;
							case byte v:
								field.SetValue(null, (byte)((IntegerValueSyntax)value.Value).Value);
								break;
							case short v:
								field.SetValue(null, (short)((IntegerValueSyntax)value.Value).Value);
								break;
							case ushort v:
								field.SetValue(null, (ushort)((IntegerValueSyntax)value.Value).Value);
								break;
							case int v:
								field.SetValue(null, (int)((IntegerValueSyntax)value.Value).Value);
								break;
							case uint v:
								field.SetValue(null, (uint)((IntegerValueSyntax)value.Value).Value);
								break;
							case ulong v:
								field.SetValue(null, unchecked((ulong)((IntegerValueSyntax)value.Value).Value));
								break;
							case float v:
								field.SetValue(null, (float)((FloatValueSyntax)value.Value).Value);
								break;
							case double v:
								field.SetValue(null, (double)((FloatValueSyntax)value.Value).Value);
								break;
							case bool v:
								field.SetValue(null, (bool)((BooleanValueSyntax)value.Value).Value);
								break;
							default:
								if (fieldValue.GetType().IsEnum) {
									var enumNames = fieldValue.GetType().GetEnumNames();
									var values = fieldValue.GetType().GetEnumValues();

									var configValue = ((StringValueSyntax)value.Value).Value.Trim();

									bool found = false;
									foreach (var index in 0.Until(enumNames.Length)) {
										if (enumNames[index] == configValue) {
											field.SetValue(null, values.GetValue(index));
											found = true;
											break;
										}
									}
									if (!found)
										throw new InvalidDataException($"Unknown Enumeration Value Type '{summedClass}.{keyString}' = '{value.Value.ToString()}'");

									break;
								}
								else {
									throw new InvalidDataException($"Unknown Configuration Value Type '{summedClass}.{keyString}' = '{value.Value.ToString()}'");
								}
						}
					}
				}
			}
			catch (Exception ex) {
				ex.PrintWarning();
				return false;
			}

			return true;
		}

		private void SaveClass(Type type, DocumentSyntax document, KeySyntax key = null) {
			bool root = (type == typeof(Config));

			key = key ?? new KeySyntax(type.Name);

			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			var children = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

			var table = new TableSyntax(key);
			var tableItems = table.Items;

			foreach (var field in fields) {
				if (field.IsPrivate || field.IsInitOnly || !field.IsStatic || field.IsLiteral)
					continue;

				if (field.GetCustomAttribute<Config.FileIgnoreAttribute>() != null) {
					continue;
				}

				ValueSyntax value = null;
				object fieldValue = field.GetValue(null);

				switch (fieldValue) {
					case string v:
						value = new StringValueSyntax(v); break;
					case sbyte v:
						value = new IntegerValueSyntax(v); break;
					case byte v:
						value = new IntegerValueSyntax(v); break;
					case short v:
						value = new IntegerValueSyntax(v); break;
					case ushort v:
						value = new IntegerValueSyntax(v); break;
					case int v:
						value = new IntegerValueSyntax(v); break;
					case uint v:
						value = new IntegerValueSyntax(v); break;
					case ulong v:
						value = new IntegerValueSyntax(unchecked((long)v)); break;
					case float v:
						value = new FloatValueSyntax(v); break;
					case double v:
						value = new FloatValueSyntax(v); break;
					case bool v:
						value = new BooleanValueSyntax(v); break;
				}

				if (value == null && fieldValue.GetType().IsEnum) {
					value = new StringValueSyntax(fieldValue.GetType().GetEnumName(fieldValue));
				}

				if (value == null)
					continue;

				tableItems.Add(new KeyValueSyntax(
					field.Name,
					value
				));
			}

			foreach (var child in children) {
				if (child.IsNestedPrivate)
					continue;
				if (child.GetCustomAttribute<Config.FileIgnoreAttribute>() != null) {
					continue;
				}
				var childKey = new KeySyntax(typeof(Config).Name);
				var parentKey = key.ToString().Split('.');
				if (parentKey.Length != 0) {
					parentKey[0] = null;
				}
				foreach (var subKey in parentKey) {
					if (subKey == null)
						continue;
					childKey.DotKeys.Add(new DottedKeyItemSyntax(subKey));
				}
				childKey.DotKeys.Add(new DottedKeyItemSyntax(child.Name));
				SaveClass(child, document, childKey);
			}

			if (table.Items.ChildrenCount != 0) {
				document.Tables.Add(table);
			}
		}

		private bool SaveConfig(string ConfigPath) {
			try {
				var Document = new DocumentSyntax();

				SaveClass(typeof(Config), Document);

				using (var writer = File.CreateText(ConfigPath)) {
					Document.WriteTo(writer);
				}
			}
			catch (Exception ex) {
				ex.PrintWarning();
				return false;
			}
			return true;
		}

		private void ConfigureHarmony() {
			var instance = HarmonyInstance.Create($"DigitalCarbide.${Config.ModuleName}");
			instance.ApplyPatches();
		}

		private static void OnButtonPressed (object sender, ButtonPressedEventArgs args) {
			switch (args.Button) {
				case Config.Debug.CacheDump.Button:
					if (Config.Debug.CacheDump.Enabled) {
						Debug.DumpMemory();
					}
					break;
			}

			if (args.Button == Config.ToggleButton)
				Config.Enabled = !Config.Enabled;
		}
	}
}
