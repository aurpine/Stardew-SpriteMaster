﻿using LinqFasterer;
using SpriteMaster.Configuration;
using SpriteMaster.Extensions;
using System;
using System.Reflection;

namespace SpriteMaster;

// TODO : make a proper Version class

internal static class Versioning {
    private static T? GetAssemblyAttribute<T>() where T : Attribute => typeof(SpriteMaster).Assembly.GetCustomAttribute<T>();

    [Attributes.Ignore]
    internal static readonly string CurrentVersion =
        System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion ??
        throw new BadImageFormatException($"Could not extract version from assembly {typeof(SMConfig).Assembly.FullName ?? typeof(SMConfig).Assembly.ToString()}");

    [Attributes.Ignore]
    internal static readonly Version AssemblyVersion =
        SpriteMaster.Assembly.GetName().Version ??
        throw new BadImageFormatException($"Could not extract version from assembly {typeof(SMConfig).Assembly.FullName ?? typeof(SMConfig).Assembly.ToString()}");

    internal static readonly string ChangeList = GetAssemblyAttribute<ChangeListAttribute>()?.Value ?? "local";
    internal static readonly string BuildComputerName = GetAssemblyAttribute<BuildComputerNameAttribute>()?.Value ?? "unknown";
    internal static readonly string FullVersion = GetAssemblyAttribute<FullVersionAttribute>()?.Value ?? CurrentVersion;

    internal static bool IsOutdated(string configVersion) {
        string referenceVersion = Config.ClearConfigBefore;

        var configStrArray = configVersion.Split('.');
        var referenceStrArray = referenceVersion.Split('.');

        try {
            int maxLen = Math.Max(configStrArray.Length, referenceStrArray.Length);
            for (int i = 0; i < maxLen; ++i) {
                if (configStrArray.Length <= i || configStrArray[i].IsEmpty()) {
                    return true;
                }
                if (referenceStrArray.Length <= i || referenceStrArray[i].IsEmpty()) {
                    return false;
                }

                var configElement = int.Parse(configStrArray[i]);
                var referenceElement = int.Parse(referenceStrArray[i]);

                if (configElement > referenceElement) {
                    return false;
                }

                if (configElement < referenceElement) {
                    return true;
                }
            }
        }
        catch {
            return true;
        }
        return false;
    }

    internal static string StringHeader =>
        $"Clear Glasses {FullVersion} build {AssemblyVersion.Revision} ({Config.BuildConfiguration}, {ChangeList}, {BuildComputerName})";
}
