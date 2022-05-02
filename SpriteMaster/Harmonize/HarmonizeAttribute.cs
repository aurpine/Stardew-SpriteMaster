﻿using LinqFasterer;
using System;
using System.Reflection;
using static SpriteMaster.Harmonize.Harmonize;

namespace SpriteMaster.Harmonize;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
class HarmonizeAttribute : Attribute {
	internal readonly Type? Type;
	internal readonly string? Name;
	internal readonly int PatchPriority;
	internal readonly Fixation PatchFixation;
	internal readonly Generic GenericType;
	internal readonly bool Instance;
	internal readonly Platform ForPlatform;
	internal readonly bool Critical;
	internal readonly string? ForMod;

	internal static bool CheckPlatform(Platform platform) => platform switch {
		Platform.All => true,
		Platform.Windows => Runtime.IsWindows,
		Platform.Linux => Runtime.IsLinux,
		Platform.Macintosh => Runtime.IsMacintosh,
		Platform.Unix => Runtime.IsUnix,
		Platform.XNA => Runtime.IsXNA,
		Platform.MonoGame => Runtime.IsMonoGame,
		_ => throw new ArgumentOutOfRangeException(nameof(ForPlatform)),
	};

	internal bool CheckPlatform() => CheckPlatform(ForPlatform);

	private static Assembly? GetAssembly(string name, bool critical, string? forMod) {
		// If it's a mod patch, and the mod doesn't exist, don't bother searching for the assembly
		if (forMod is not null && SpriteMaster.Self.Helper.ModRegistry.Get(forMod) is null) {
			return null;
		}
		
		if (Runtime.IsMonoGame && name.StartsWith("Microsoft.Xna.Framework")) {
			name = "MonoGame.Framework";
		}

		try {
			return AppDomain.CurrentDomain.GetAssemblies().SingleF(assembly => assembly.GetName().Name == name);
		}
		catch {
			Debug.ConditionalError(critical, $"Assembly Not Found For Harmonize: {name}");
			if (critical) {
				throw;
			}
		}
		return null;
	}

	private static Type? ResolveType(Assembly assembly, Type? parent, string[] type, int offset = 0) {
		if (parent is null) {
			return null;
		}

		var foundType = parent.GetNestedType(type[offset], BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (foundType is null) {
			return parent;
		}
		offset += 1;
		if (offset >= type.Length)
			return foundType;
		else
			return ResolveType(assembly, foundType, type, offset);
	}

	private static Type? ResolveType(Assembly? assembly, string[] type, int offset = 0) => assembly is null ? null : ResolveType(assembly, assembly.GetType(type[0], true), type, offset + 1);

	internal HarmonizeAttribute(
		Type? type,
		string? method,
		Fixation fixation = Fixation.Prefix,
		PriorityLevel priority = PriorityLevel.Average,
		Generic generic = Generic.None,
		bool instance = true,
		bool critical = true,
		Platform platform = Platform.All,
		string? forMod = null
	) {
		Type = type;
		Name = method;
		PatchPriority = (int)priority;
		PatchFixation = fixation;
		GenericType = generic;
		Instance = instance;
		ForPlatform = platform;
		Critical = critical;
		ForMod = forMod;
	}

	internal HarmonizeAttribute(
		string assembly,
		string type,
		string method,
		Fixation fixation = Fixation.Prefix,
		PriorityLevel priority = PriorityLevel.Average,
		Generic generic = Generic.None,
		bool instance = true,
		bool critical = true,
		Platform platform = Platform.All,
		string? forMod = null
	) :
		this(
			type: CheckPlatform(platform) ? GetAssembly(assembly, critical: critical, forMod: forMod)?.GetType(type, true) : null,
			method: method,
			fixation: fixation,
			priority: priority,
			generic: generic,
			instance: instance,
			critical: critical,
			platform: platform,
			forMod: forMod
		) { }

	internal HarmonizeAttribute(
		Type parent,
		string type,
		string method,
		Fixation fixation = Fixation.Prefix,
		PriorityLevel priority = PriorityLevel.Average,
		Generic generic = Generic.None,
		bool instance = true,
		bool critical = true,
		Platform platform = Platform.All,
		string? forMod = null
	) :
		this(
			type: CheckPlatform(platform) ? parent.Assembly.GetType(type, true) : null,
			method: method,
			fixation: fixation,
			priority: priority,
			generic: generic,
			instance: instance,
			critical: critical,
			platform: platform,
			forMod: forMod
		) { }

	internal HarmonizeAttribute(
		Type parent,
		string[] type,
		string method,
		Fixation fixation = Fixation.Prefix,
		PriorityLevel priority = PriorityLevel.Average,
		Generic generic = Generic.None,
		bool instance = true,
		bool critical = true,
		Platform platform = Platform.All,
		string? forMod = null
	) :
		this(
			type: CheckPlatform(platform) ? ResolveType(parent.Assembly, type) : null,
			method: method,
			fixation: fixation,
			priority: priority,
			generic: generic,
			instance: instance,
			critical: critical,
			platform: platform,
			forMod: forMod
		) { }

	internal HarmonizeAttribute(
		string assembly,
		string[] type,
		string method,
		Fixation fixation = Fixation.Prefix,
		PriorityLevel priority = PriorityLevel.Average,
		Generic generic = Generic.None,
		bool instance = true,
		bool critical = true,
		Platform platform = Platform.All,
		string? forMod = null
	) :
		this(
			type: CheckPlatform(platform) ? ResolveType(GetAssembly(assembly, critical: critical, forMod: forMod), type) : null,
			method: method,
			fixation: fixation,
			priority: priority,
			generic: generic,
			instance: instance,
			critical: critical,
			platform: platform,
			forMod: forMod
		) { }

	internal HarmonizeAttribute(
		string method,
		Fixation fixation = Fixation.Prefix,
		PriorityLevel priority = PriorityLevel.Average,
		Generic generic = Generic.None,
		bool instance = true,
		bool critical = true,
		Platform platform = Platform.All,
		string? forMod = null
	) :
		this(
			type: null,
			method: method,
			fixation: fixation,
			priority: priority,
			generic: generic,
			instance: instance,
			critical: critical,
			platform: platform,
			forMod: forMod
		) { }

	internal HarmonizeAttribute(
		Fixation fixation = Fixation.Prefix,
		PriorityLevel priority = PriorityLevel.Average,
		Generic generic = Generic.None,
		bool instance = true,
		bool critical = true,
		Platform platform = Platform.All,
		string? forMod = null
	) :
		this(
			type: null,
			method: null,
			fixation: fixation,
			priority: priority,
			generic: generic,
			instance: instance,
			critical: critical,
			platform: platform,
			forMod: forMod
		) { }
}
