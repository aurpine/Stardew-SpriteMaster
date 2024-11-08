﻿using HarmonyLib;
using LinqFasterer;
using Microsoft.Xna.Framework.Graphics;
using SpriteMaster.Configuration;
using SpriteMaster.Extensions;
using SpriteMaster.Extensions.Reflection;
using SpriteMaster.Harmonize.Patches.Game;
using SpriteMaster.Harmonize.Patches.PSpriteBatch.Patch;
using SpriteMaster.Resample;
using SpriteMaster.Types;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using static SpriteMaster.Harmonize.Harmonize;

namespace SpriteMaster.Harmonize.Patches.PSpriteBatch;

[SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Harmony")]
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Harmony")]
internal static class PlatformRenderBatch {
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowModeUnimplementedException<T>(string name, TextureAddressMode addressMode) =>
        throw new NotImplementedException($"{name} {addressMode} is unimplemented");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static T ThrowModeUnimplementedException<T>(string name, TextureFilter filter) =>
        throw new NotImplementedException($"{name} {filter} is unimplemented");

    private static SamplerState GetSamplerState(TextureAddressMode addressMode, TextureFilter filter) {
        return (addressMode, filter) switch {
            (TextureAddressMode.Wrap, TextureFilter.Point) => SamplerState.PointWrap,
            (TextureAddressMode.Wrap, TextureFilter.Linear) => SamplerState.LinearWrap,
            (TextureAddressMode.Wrap, TextureFilter.Anisotropic) => SamplerState.AnisotropicWrap,
            (TextureAddressMode.Clamp, TextureFilter.Point) => SamplerState.PointClamp,
            (TextureAddressMode.Clamp, TextureFilter.Linear) => SamplerState.LinearClamp,
            (TextureAddressMode.Clamp, TextureFilter.Anisotropic) => SamplerState.AnisotropicClamp,
            (TextureAddressMode.Border, TextureFilter.Point) => DrawState.SamplerStateExt.PointBorder.Value,
            (TextureAddressMode.Border, TextureFilter.Linear) => DrawState.SamplerStateExt.LinearBorder.Value,
            (TextureAddressMode.Border, TextureFilter.Anisotropic) => DrawState.SamplerStateExt.AnisotropicBorder.Value,
            (TextureAddressMode.Mirror, TextureFilter.Point) => DrawState.SamplerStateExt.PointMirror.Value,
            (TextureAddressMode.Mirror, TextureFilter.Linear) => DrawState.SamplerStateExt.LinearMirror.Value,
            (TextureAddressMode.Mirror, TextureFilter.Anisotropic) => DrawState.SamplerStateExt.AnisotropicMirror.Value,
            (_, TextureFilter.Point) => ThrowModeUnimplementedException<SamplerState>(nameof(TextureAddressMode), addressMode),
            (_, TextureFilter.Linear) => ThrowModeUnimplementedException<SamplerState>(nameof(TextureAddressMode), addressMode),
            (_, TextureFilter.Anisotropic) => ThrowModeUnimplementedException<SamplerState>(nameof(TextureAddressMode), addressMode),
            _ => ThrowModeUnimplementedException<SamplerState>(nameof(TextureFilter), filter),
        };
    }

    private static SamplerState GetNewSamplerState(Texture? texture, SamplerState reference) {
        if (!Config.DrawState.IsSetLinear) {
            return reference;
        }

        bool isInternalTexture = texture is InternalTexture2D;
        bool isLighting = !isInternalTexture && (texture?.NormalizedName().StartsWith(@"LooseSprites\Lighting\") ?? false);

        if (!isInternalTexture && !isLighting && !Config.DrawState.IsSetLinearUnresampled) {
            return reference;
        }

        IScalerInfo? scalerInfo = null;
        if (isInternalTexture && texture is ManagedTexture2D managedTexture) {
            scalerInfo = managedTexture.SpriteInstance.ScalerInfo;
        }

        var preferredFilter = scalerInfo?.Filter;

        if (preferredFilter == null) {
            return reference;
        }

        return reference.AddressU switch {
            TextureAddressMode.Wrap when reference.AddressV == TextureAddressMode.Wrap => GetSamplerState(
                addressMode: TextureAddressMode.Wrap, filter: (TextureFilter)preferredFilter
            ),
            TextureAddressMode.Border when reference.AddressV == TextureAddressMode.Border => GetSamplerState(
                addressMode: TextureAddressMode.Border, filter: (TextureFilter)preferredFilter
            ),
            TextureAddressMode.Mirror when reference.AddressV == TextureAddressMode.Mirror => GetSamplerState(
                addressMode: TextureAddressMode.Mirror, filter: (TextureFilter)preferredFilter
            ),
            _ => GetSamplerState(addressMode: TextureAddressMode.Clamp, filter: (TextureFilter)preferredFilter)
        };
    }

    internal readonly record struct States(SamplerState? SamplerState, BlendState? BlendState);

    /*
	[HarmonizeTranspile(
		type: typeof(SpriteBatcher),
		"FlushVertexArray",
		argumentTypes: new[] { typeof(int), typeof(int), typeof(Effect), typeof(Texture) }
	)]
	public static IEnumerable<CodeInstruction> FlushVertexArrayTranspiler(IEnumerable<CodeInstruction> instructions) {
		var newMethod = new Action<GraphicsDevice, PrimitiveType, VertexPositionColorTexture[], int, int, short[], int, int, VertexDeclaration>(GL.GraphicsDeviceExt.DrawUserIndexedPrimitivesFlushVertexArray).Method;

		var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();

		bool applied = false;

		static bool IsCall(OpCode opCode) {
			return opCode.Value == OpCodes.Call.Value || opCode.Value == OpCodes.Calli.Value || opCode.Value == OpCodes.Callvirt.Value;
		}

		IEnumerable<CodeInstruction> ApplyPatch() {
			foreach (var instruction in codeInstructions) {
				if (
					!IsCall(instruction.opcode) ||
					instruction.operand is not MethodInfo callee ||
					callee.Name != "DrawUserIndexedPrimitives"
				) {
					yield return instruction;
					continue;
				}

				yield return new(OpCodes.Call, newMethod) {
					labels = instruction.labels,
					blocks = instruction.blocks
				};
				//yield return new(OpCodes.Pop);
				applied = true;
			}
		}

		var result = ApplyPatch().ToArray();

		if (!applied) {
			Debug.Error("Could not apply SpriteBatcher FlushVertexArray patch.");
		}

		return result;
	}
	*/

    [Harmonize(
        "Microsoft.Xna.Framework.Graphics",
        "Microsoft.Xna.Framework.Graphics.SpriteBatcher",
        "FlushVertexArray",
        Fixation.Prefix,
        PriorityLevel.First,
        platform: Platform.MonoGame
    )]
    public static void OnFlushVertexArray(
        SpriteBatcher __instance,
        int start,
        int end,
        Effect? effect,
        Texture? texture,
        GraphicsDevice? ____device,
        ref States __state
    ) {
        if (!Config.IsEnabled) {
            return;
        }

        SamplerState? originalSamplerState = null;
        BlendState? originalBlendState = null;

        try {
            using var watchdogScoped = WatchDog.WatchDog.ScopedWorkingState;

            {
                var originalState = ____device?.SamplerStates[0] ?? SamplerState.PointClamp;

                var newState = GetNewSamplerState(texture, originalState);

                if (newState != originalState && ____device?.SamplerStates is not null) {
                    originalSamplerState = originalState;
                    ____device.SamplerStates[0] = newState;
                }
                else {
                    originalSamplerState = null;
                }
            }
            {
                if (____device is not null) {
                    var originalState = ____device.BlendState;
                    if (texture == Line.LineTexture.Value) {
                        ____device.BlendState = BlendState.AlphaBlend;
                    }
                    originalBlendState = originalState;
                }
                else {
                    originalBlendState = null;
                }
            }
        }
        catch (Exception ex) {
            ex.PrintError();
        }

        __state = new(originalSamplerState, originalBlendState);
    }

    [Harmonize(
        "Microsoft.Xna.Framework.Graphics",
        "Microsoft.Xna.Framework.Graphics.SpriteBatcher",
        "FlushVertexArray",
        Fixation.Postfix,
        PriorityLevel.Last,
        platform: Platform.MonoGame
    )]
    public static void OnFlushVertexArray(
        SpriteBatcher __instance,
        int start,
        int end,
        Effect? effect,
        Texture? texture,
        GraphicsDevice? ____device,
        States __state
    ) {
        if (!Config.IsEnabled) {
            return;
        }

        try {
            using var watchdogScoped = WatchDog.WatchDog.ScopedWorkingState;

            if (__state.SamplerState is not null && ____device?.SamplerStates is not null && __state.SamplerState != ____device.SamplerStates[0]) {
                ____device.SamplerStates[0] = __state.SamplerState;
            }
            if (__state.BlendState is not null && ____device?.BlendState is not null && __state.BlendState != ____device.BlendState) {
                ____device.BlendState = __state.BlendState;
            }
        }
        catch (Exception ex) {
            ex.PrintError();
        }
    }

    private static bool EnsureArrayCapacityEnabled = true;

    private static FieldInfo? SpriteBatcher__index = typeof(SpriteBatcher).GetField("_index", BindingFlags.NonPublic | BindingFlags.Instance);

    [Harmonize(
        "Microsoft.Xna.Framework.Graphics",
        "Microsoft.Xna.Framework.Graphics.SpriteBatcher",
        "EnsureArrayCapacity",
        Fixation.Prefix,
        PriorityLevel.Last,
        platform: Platform.MonoGame
    )]
    public static bool OnEnsureArrayCapacity(SpriteBatcher __instance, int numBatchItems) {
        if (!Config.IsEnabled || !EnsureArrayCapacityEnabled) {
            //var field = typeof(SpriteBatcher).GetField("_index", BindingFlags.NonPublic | BindingFlags.Instance);
            //short[] _index = (short[]) field?.GetValue(__instance);
            if (((short[])SpriteBatcher__index?.GetValue(__instance)) == GL.GraphicsDeviceExt.SpriteBatcherValues.Indices16) {
                //field?.SetValue(__instance, null);
                SpriteBatcher__index.SetValue(__instance, null);
            }
            return true;
        }

        try {
            EnsureIndexCapacity(__instance, numBatchItems);
            EnsureVertexCapacity(__instance, numBatchItems);

            return false;
        }
        catch (Exception ex) when (ex is MemberAccessException or InvalidCastException) {
            Debug.Error($"Disabling {nameof(OnEnsureArrayCapacity)} patch", ex);
            EnsureArrayCapacityEnabled = false;

            if (((short[])SpriteBatcher__index?.GetValue(__instance)) == GL.GraphicsDeviceExt.SpriteBatcherValues.Indices16) {
                SpriteBatcher__index?.SetValue(__instance, null);
            }
            return true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnsureIndexCapacity(SpriteBatcher @this, int numBatchItems) {
        SpriteBatcher__index?.SetValue(@this, GL.GraphicsDeviceExt.SpriteBatcherValues.Indices16);
    }

    private static FieldInfo? SpriteBatcher__batcher = typeof(SpriteBatcher).GetField("_batcher", BindingFlags.NonPublic | BindingFlags.Instance);
    private static FieldInfo? SpriteBatcher__vertexArray = typeof(SpriteBatcher).GetField("_vertexArray", BindingFlags.NonPublic | BindingFlags.Instance);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnsureVertexCapacity(SpriteBatcher @this, int numBatchItems) {
        if (Game1.spriteBatch is null || @this == SpriteBatcher__batcher?.GetValue(Game1.spriteBatch)) {
            //if (Game1.spriteBatch is null || @this == ((dynamic) Game1.spriteBatch)._batcher) {
            numBatchItems = 5461;
        }
        int neededCapacity = (int)((uint)numBatchItems * 4u);

        //var _vertexArrayField = typeof(SpriteBatcher).GetField("_vertexArray", BindingFlags.NonPublic | BindingFlags.Instance);
        //var _vertexArray = _vertexArrayField?.GetValue(@this);

        var _vertexArray = SpriteBatcher__vertexArray?.GetValue(@this);

        if (_vertexArray is null || ((VertexPositionColorTexture[])_vertexArray).Length < neededCapacity) {
            //((dynamic)@this)._vertexArray = GC.AllocateUninitializedArray<VertexPositionColorTexture>(neededCapacity, pinned: true);
            SpriteBatcher__vertexArray?.SetValue(@this, GC.AllocateUninitializedArray<VertexPositionColorTexture>(neededCapacity, pinned: true));
        }
    }
}
