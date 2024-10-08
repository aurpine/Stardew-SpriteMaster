﻿using JetBrains.Annotations;
using System;
using static SpriteMaster.Harmonize.Harmonize;

namespace SpriteMaster.Harmonize;

[MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class HarmonizeTranspileAttribute : HarmonizeAttribute {
    internal HarmonizeTranspileAttribute(
        Type? type,
        string? method,
        Type[] argumentTypes,
        Type[]? genericTypes = null,
        Generic generic = Generic.None,
        bool instance = true,
        bool critical = true,
        Platform platform = Platform.All,
        string? forMod = null
    ) : base(
        type: type,
        method: method,
        argumentTypes: argumentTypes,
        generic: generic,
        fixation: Fixation.Transpile,
        instance: instance,
        critical: critical,
        platform: platform,
        forMod: forMod,
        genericTypes: genericTypes
    ) {
    }

    internal HarmonizeTranspileAttribute(
        string assembly,
        string type,
        string method,
        Type[] argumentTypes,
        Type[]? genericTypes = null,
        Generic generic = Generic.None,
        bool instance = true,
        bool critical = true,
        Platform platform = Platform.All,
        string? forMod = null
    ) : base(
        assembly: assembly,
        type: type,
        method: method,
        argumentTypes: argumentTypes,
        generic: generic,
        fixation: Fixation.Transpile,
        instance: instance,
        critical: critical,
        platform: platform,
        forMod: forMod,
        genericTypes: genericTypes
    ) { }

    internal HarmonizeTranspileAttribute(
        Type parent,
        string type,
        string method,
        Type[] argumentTypes,
        Type[]? genericTypes = null,
        Generic generic = Generic.None,
        bool instance = true,
        bool critical = true,
        Platform platform = Platform.All,
        string? forMod = null
    ) : base(
        parent: parent,
        type: type,
        method: method,
        argumentTypes: argumentTypes,
        generic: generic,
        fixation: Fixation.Transpile,
        instance: instance,
        critical: critical,
        platform: platform,
        forMod: forMod,
        genericTypes: genericTypes
    ) { }

    internal HarmonizeTranspileAttribute(
        string type,
        string method,
        Type[] argumentTypes,
        Type[]? genericTypes = null,
        Generic generic = Generic.None,
        bool instance = true,
        bool critical = true,
        Platform platform = Platform.All,
        string? forMod = null
    ) : base(
        type: type,
        method: method,
        argumentTypes: argumentTypes,
        generic: generic,
        fixation: Fixation.Transpile,
        instance: instance,
        critical: critical,
        platform: platform,
        forMod: forMod,
        genericTypes: genericTypes
    ) { }

    internal HarmonizeTranspileAttribute(
        Type parent,
        string[] type,
        string method,
        Type[] argumentTypes,
        Type[]? genericTypes = null,
        Generic generic = Generic.None,
        bool instance = true,
        bool critical = true,
        Platform platform = Platform.All,
        string? forMod = null
    ) : base(
        parent: parent,
        type: type,
        method: method,
        argumentTypes: argumentTypes,
        generic: generic,
        fixation: Fixation.Transpile,
        instance: instance,
        critical: critical,
        platform: platform,
        forMod: forMod,
        genericTypes: genericTypes
    ) { }

    internal HarmonizeTranspileAttribute(
        string assembly,
        string[] type,
        string method,
        Type[] argumentTypes,
        Type[]? genericTypes = null,
        Generic generic = Generic.None,
        bool instance = true,
        bool critical = true,
        Platform platform = Platform.All,
        string? forMod = null
    ) : base(
        assembly: assembly,
        type: type,
        method: method,
        argumentTypes: argumentTypes,
        generic: generic,
        fixation: Fixation.Transpile,
        instance: instance,
        critical: critical,
        platform: platform,
        forMod: forMod,
        genericTypes: genericTypes
    ) { }

    internal HarmonizeTranspileAttribute(
        string method,
        Type[] argumentTypes,
        Type[]? genericTypes = null,
        Generic generic = Generic.None,
        bool instance = true,
        bool critical = true,
        Platform platform = Platform.All,
        string? forMod = null
    ) : base(
        method: method,
        argumentTypes: argumentTypes,
        generic: generic,
        fixation: Fixation.Transpile,
        instance: instance,
        critical: critical,
        platform: platform,
        forMod: forMod,
        genericTypes: genericTypes
    ) { }

    internal HarmonizeTranspileAttribute(
        Type[] argumentTypes,
        Type[]? genericTypes = null,
        Generic generic = Generic.None,
        bool instance = true,
        bool critical = true,
        Platform platform = Platform.All,
        string? forMod = null
    ) : base(
        argumentTypes: argumentTypes,
        generic: generic,
        fixation: Fixation.Transpile,
        instance: instance,
        critical: critical,
        platform: platform,
        forMod: forMod,
        genericTypes: genericTypes
    ) { }
}
