﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteMaster.Configuration;
using SpriteMaster.Extensions;
using SpriteMaster.Extensions.Reflection;
using SpriteMaster.Tasking;
using SpriteMaster.Types;
using SpriteMaster.Types.Interlocking;
using StardewValley;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using xTile.Display;

namespace SpriteMaster;

internal static partial class DrawState {
    private static class UpdateState {
        internal static readonly InterlockedULong LastUpdated = 0UL;

        // ReSharper disable once MemberHidesStaticFromOuterClass
        internal static volatile bool IsUpdatedThisFrame = false;
    }

    internal static bool IsUpdatedThisFrame {
        get => UpdateState.IsUpdatedThisFrame;
        set {
            if (value) {
                UpdateState.LastUpdated.Set(CurrentFrame);
            }
            UpdateState.IsUpdatedThisFrame = value;
        }
    }

    internal static InterlockedULong CurrentFrame = 0UL;

    private static class Defaults {
        internal static readonly SamplerState SamplerState = SamplerState.LinearClamp;
        internal static readonly BlendState BlendState = BlendState.AlphaBlend;
        internal static readonly RasterizerState RasterizerState = RasterizerState.CullCounterClockwise;
        internal const SpriteSortMode SortMode = SpriteSortMode.Deferred;
    }

    private static SamplerState MakeSamplerState(SamplerState reference, TextureAddressMode addressMode) {
        if (reference.AddressU == addressMode && reference.AddressV == addressMode) {
            return reference;
        }

        var state = reference.Clone();
        state.AddressU = state.AddressV = addressMode;
        return state;
    }

    internal static class SamplerStateExt {
        internal static readonly Lazy<SamplerState> AnisotropicBorder = new(() => MakeSamplerState(SamplerState.AnisotropicClamp, TextureAddressMode.Border));
        internal static readonly Lazy<SamplerState> LinearBorder = new(() => MakeSamplerState(SamplerState.LinearClamp, TextureAddressMode.Border));
        internal static readonly Lazy<SamplerState> PointBorder = new(() => MakeSamplerState(SamplerState.PointClamp, TextureAddressMode.Border));
        internal static readonly Lazy<SamplerState> AnisotropicMirror = new(() => MakeSamplerState(SamplerState.AnisotropicClamp, TextureAddressMode.Mirror));
        internal static readonly Lazy<SamplerState> LinearMirror = new(() => MakeSamplerState(SamplerState.LinearClamp, TextureAddressMode.Mirror));
        internal static readonly Lazy<SamplerState> PointMirror = new(() => MakeSamplerState(SamplerState.PointClamp, TextureAddressMode.Mirror));
    }

    internal static SamplerState CurrentSamplerState = Defaults.SamplerState;
    internal static BlendState CurrentBlendState = Defaults.BlendState;
    internal static RasterizerState CurrentRasterizerState = Defaults.RasterizerState;
    internal static SpriteSortMode CurrentSortMode = Defaults.SortMode;

    internal static readonly Condition TriggerCollection = new(initialState: false);

    private static TimeSpan ExpectedFrameTime = GameConstants.FrameTime.TimeSpan;
    internal static bool ForceSynchronous = false;

    private static int LastFrameTimesIndex = 0;
    private static TimeSpan[] LastFrameTimesCPU = new TimeSpan[64];
    private static TimeSpan[] LastFrameTimesTotal = new TimeSpan[64];
    private static readonly System.Diagnostics.Stopwatch FrameStopwatch = System.Diagnostics.Stopwatch.StartNew();
    private static readonly System.Diagnostics.Stopwatch RealFrameStopwatch = System.Diagnostics.Stopwatch.StartNew();

    private const int BaselineFrameTimeRunningCount = 20;
    private static TimeSpan BaselineFrameTime = TimeSpan.Zero;

    internal static class Statistics {
        internal static uint DrawCalls = 0u;

        internal static void Reset() {
            DrawCalls = 0u;
        }
    }

    internal static void UpdateDeviceManager(GraphicsDeviceManager manager) {
        var rate = manager.GetField("game")?.GetProperty<TimeSpan>("TargetElapsedTime");
        ExpectedFrameTime = rate.GetValueOrDefault(ExpectedFrameTime);
    }

    private static GraphicsDevice? PreviousDevice = null;
    internal static GraphicsDevice Device {
        get {
            UpdateDevice();
            return PreviousDevice!;
        }
    }
    internal static void UpdateDevice() {
        var currentDevice = Game1.graphics.GraphicsDevice;
        if (currentDevice != PreviousDevice) {
            PreviousDevice = currentDevice;
            Harmonize.Patches.Game.HoeDirt.OnNewGraphicsDevice(currentDevice);
        }
    }

    internal static bool PushedUpdateWithin(int frames) => (long)(CurrentFrame - UpdateState.LastUpdated) <= frames;

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static TimeSpan RemainingFrameTime(float multiplier = 1.0f, TimeSpan? offset = null) {
        var actualRemainingTime = ActualRemainingFrameTime();
        return (actualRemainingTime - (BaselineFrameTime + (offset ?? TimeSpan.Zero))).Multiply(multiplier);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    private static TimeSpan ActualRemainingFrameTime() => ExpectedFrameTime - FrameStopwatch.Elapsed;

    internal static void OnBeginDraw() {
        RealFrameStopwatch.Restart();
    }

    internal static void OnPresent() {
        Thread.CurrentThread.Priority = ThreadPriority.Highest;

        using var watchdogScoped = WatchDog.WatchDog.ScopedWorkingState;

        TimeSpan? lastFrameTimeCPU = null;
        TimeSpan? lastFrameTimeTotal = null;

        if (Config.Debug.DisplayFrameTime) {
            {
                TimeSpan sumTime = TimeSpan.Zero;
                foreach (var frameTime in LastFrameTimesCPU) {
                    sumTime += frameTime;
                }

                sumTime /= LastFrameTimesCPU.Length;
                lastFrameTimeCPU = sumTime;
            }
            {
                TimeSpan sumTime = TimeSpan.Zero;
                foreach (var frameTime in LastFrameTimesTotal) {
                    sumTime += frameTime;
                }

                sumTime /= LastFrameTimesTotal.Length;
                lastFrameTimeTotal = sumTime;
            }
        }

        Debug.Mode.Draw(lastFrameTimeCPU, lastFrameTimeTotal);

        ++CurrentFrame;

        if (TriggerCollection.GetAndClear()) {
            ManagedSpriteInstance.PurgeTextures((Config.Garbage.RequiredFreeMemorySoft * Config.Garbage.RequiredFreeMemoryHysteresis).NearestLong());
            Garbage.Collect(compact: true, blocking: true, background: false);
        }

        if (Config.AsyncScaling.CanFetchAndLoadSameFrame || !IsUpdatedThisFrame) {
            var remaining = ActualRemainingFrameTime();
            SynchronizedTaskScheduler.Instance.Dispatch(remaining);
        }

        if (!IsUpdatedThisFrame) {
            var duration = FrameStopwatch.Elapsed;
            // Throw out garbage values.
            if (duration <= ExpectedFrameTime + ExpectedFrameTime) {
                var mean = BaselineFrameTime;
                mean -= mean / BaselineFrameTimeRunningCount;
                mean += duration / BaselineFrameTimeRunningCount;
                BaselineFrameTime = mean;

                // TODO : fix me, this doesn't work particularly well so I've disabled it.
                BaselineFrameTime = TimeSpan.Zero;
            }
        }
        else {
            IsUpdatedThisFrame = false;
        }

        if (Config.Debug.DisplayFrameTime) {
            int index = LastFrameTimesIndex++;
            LastFrameTimesIndex %= LastFrameTimesCPU.Length;
            LastFrameTimesCPU[index] = RealFrameStopwatch.Elapsed;
            LastFrameTimesTotal[index] = FrameStopwatch.Elapsed;
        }

        if (!Config.IsEnabled) {
            return;
        }

        Runtime.CorrectProcessorAffinity();

        Garbage.EphemeralCollection.Collect(CurrentFrame);
    }

    private static readonly WeakReference<xTile.Display.IDisplayDevice> LastMitigatedDevice = new(null!);
    private static readonly Type? PyDisplayDeviceType = ReflectionExt.GetTypeExt(@"PyTK.Types.PyDisplayDevice");
    private static readonly Action<IDisplayDevice, bool>? AdjustOriginSetter = PyDisplayDeviceType?.GetFieldSetter<IDisplayDevice, bool>("adjustOrigin");

    private static readonly Predicate<IDisplayDevice>? IsPyDisplayDevice =
        PyDisplayDeviceType?.GetIsDelegate<IDisplayDevice>();

    // ReSharper disable once InconsistentNaming
    private static void DisablePyTKMitigation() {
        if (!Config.Extras.ModPatches.DisablePyTKMitigation) {
            return;
        }

        if (IsPyDisplayDevice is not { } isPyDisplayDevice || AdjustOriginSetter is not { } adjustOriginSetter) {
            return;
        }

        var mapDisplayDevice = Game1.mapDisplayDevice;

        if (LastMitigatedDevice.TryGetTarget(out var lastDevice) && lastDevice == mapDisplayDevice) {
            return;
        }

        if (!isPyDisplayDevice(mapDisplayDevice)) {
            return;
        }

        adjustOriginSetter(mapDisplayDevice, false);

        LastMitigatedDevice.SetTarget(Game1.mapDisplayDevice);
    }

    [MethodImpl(Runtime.MethodImpl.Inline)]
    internal static void OnPresentPost() {
        FrameStopwatch.Restart();

        using var watchdogScoped = WatchDog.WatchDog.ScopedWorkingState;

        Core.OnDrawImpl.ResetLastDrawCache();

        // Apply the PyTK mediation here because we do not know when it might be set up
        DisablePyTKMitigation();

        Statistics.Reset();
    }

    private static bool FirstDraw = true;

    internal static void OnBegin(
        XSpriteBatch @this,
        SpriteSortMode sortMode,
        BlendState? blendState,
        SamplerState? samplerState,
        DepthStencilState? depthStencilState,
        RasterizerState? rasterizerState,
        Effect? effect,
        in Matrix transformMatrix
    ) {
        using var watchdogScoped = WatchDog.WatchDog.ScopedWorkingState;

        if (FirstDraw) {
            SpriteMaster.Self.OnFirstDraw();
            FirstDraw = false;
        }

        CurrentSortMode = sortMode;
        CurrentSamplerState = samplerState ?? Defaults.SamplerState;// ConditionallyClone(samplerState, Defaults.SamplerState);
        CurrentBlendState = blendState ?? Defaults.BlendState; // ConditionallyClone(blendState, Defaults.BlendState);
        CurrentRasterizerState = rasterizerState ?? Defaults.RasterizerState;

        CheckStates();

        var device = @this.GraphicsDevice;
        var renderTargets = device.GetRenderTargets();
        var renderTarget = renderTargets.Length != 0 ? renderTargets[0].RenderTarget : null;

        //if (renderTarget is RenderTarget2D target && target.RenderTargetUsage != RenderTargetUsage.DiscardContents) {
        //	Debug.Warning("Non-Discarding RTT");
        //}

        // If we're drawing to the system target or to the game's front buffer or ui buffer, we do not want to be in synchronized mode.
        // Otherwise, we _do_ want to be, because it might be a mod drawing to a render target, and we need to make sure that said draw
        // actually goes through, processed!
        // TODO : though it might make more sense to do the _reverse_ for that - don't resample the draws to the render target, only resample
        // draws _from_ the render target
        // We intentionally synchronize all other game targets as well, such as the lightmap, as those are not constantly updated
        if (renderTarget is null) {
            ForceSynchronous = false;
        }
        else if (renderTarget == Game1.game1.uiScreen || renderTarget == Game1.game1.screen) {
            ForceSynchronous = false;
        }
        else {
            ForceSynchronous = true;
        }
    }
}
