﻿using HarmonyLib;
using JetBrains.Annotations;
using LinqFasterer;
using Microsoft.Xna.Framework.Input;
using SpriteMaster.Caching;
using SpriteMaster.Configuration;
using SpriteMaster.Experimental;
using SpriteMaster.Extensions;
using SpriteMaster.GL;
using SpriteMaster.Harmonize;
using SpriteMaster.Harmonize.Patches.Game;
using SpriteMaster.Metadata;
using SpriteMaster.Tasking;
using SpriteMaster.Types;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpriteMaster;

public sealed class SpriteMaster : Mod {
    internal static Assembly Assembly => typeof(SpriteMaster).Assembly;
    private const string UniqueId = "aurpine.ClearGlasses";
    private static string ModDirectory => Self?.Helper?.DirectoryPath ?? Path.GetDirectoryName(Assembly.Location) ?? Assembly.Location;

    internal static SpriteMaster Self { get; private set; } = default!;
    private const string ConfigName = "config.toml";

    internal readonly MemoryMonitor.Monitor MemoryMonitor;

    private readonly Lazy<Harmony> HarmonyInstance = new(() => new(UniqueId));

    [UsedImplicitly]
    public SpriteMaster() {
        Self.AssertNull();
        Self = this;

        _ = ThreadingExt.IsMainThread;

        Runtime.CorrectProcessorAffinity();

        DirectoryCleanup.Cleanup();

        GLExt.EnableDebugging();

        Initialize();

        Garbage.EnterNonInteractive();

        //SteamHelper.Init();

        MemoryMonitor = new();

        var assemblyPath = Assembly.Location;
        assemblyPath = Path.GetDirectoryName(assemblyPath);

        // Compress our own directory
        if (assemblyPath is not null) {
            DirectoryExt.CompressDirectory(assemblyPath, force: true);
        }

        TextureFileCache.Precache();
    }

    private void InitializeConfig() {
        Config.SetPath(Path.Combine(ModDirectory, ConfigName));

        Config.DefaultConfig = new MemoryStream();
        Serialize.Save(Config.DefaultConfig, leaveOpen: true);

        if (!Config.IgnoreConfig) {
            Serialize.Load(Config.Path);
        }

        if (Versioning.IsOlderThan(Config.ConfigVersion, Config.ClearConfigBefore)) {
            Debug.Info($"config.toml is out of date ({Config.ConfigVersion} < {Config.ClearConfigBefore}), rewriting it.");

            Serialize.Load(Config.DefaultConfig, retain: true);
            Config.DefaultConfig.Position = 0;
            Config.ConfigVersion = Versioning.CurrentVersion.FileVersion!;
        }

        if (Versioning.IsOlderThan(Config.ConfigVersion, "0.3.1")) {
            Debug.Info($"Applying config hotfix for 0.3.1 (found config {Config.ConfigVersion})");

            // Apply fix for lumisteria tilesheets
            Config.Resample.Blacklist = new(Config.Resample.DefaultBlacklist);

            // Apply fix for corrupted original textures
            Config.Extras.OpenGL.Enabled = false;
            Config.Extras.OpenGL.OptimizeTexture2DSetData = false;

            Config.ConfigVersion = Versioning.CurrentVersion.FileVersion!;
        }

        static Regex ProcessTexturePattern(string pattern) {
            pattern = pattern.StartsWith('@') ?
                pattern[1..] :
                $"^{Regex.Escape(pattern)}";
            return new(pattern, RegexOptions.Compiled);
        }

        static Config.TextureRef[] ProcessTextureRefs(List<string> textureRefStrings) {
            // handle sliced textures. At some point I will add struct support.
            var result = new Config.TextureRef[textureRefStrings.Count];
            for (int i = 0; i < result.Length; ++i) {
                var slicedTexture = textureRefStrings[i];
                //@"LooseSprites\Cursors::0,640:2000,256"
                var elements = slicedTexture.Split("::", 2);
                var texture = elements[0];
                var bounds = Bounds.Empty;
                if (elements.Length > 1) {
                    try {
                        var boundElements = elements[1].Split(':');
                        var offsetElements = (boundElements.ElementAtOrDefaultF(0) ?? "0,0").Split(',', 2);
                        var extentElements = (boundElements.ElementAtOrDefaultF(1) ?? "0,0").Split(',', 2);

                        var offset = new Vector2I(int.Parse(offsetElements[0]), int.Parse(offsetElements[1]));
                        var extent = new Vector2I(int.Parse(extentElements[0]), int.Parse(extentElements[1]));

                        bounds = new(offset, extent);
                    }
                    catch {
                        Debug.Error($"Invalid SlicedTexture Bounds: '{elements[1]}'");
                    }
                }
                result[i] = new(ProcessTexturePattern(texture), bounds);
            }
            return result;
        }

        Config.Resample.SlicedTexturesS = ProcessTextureRefs(Config.Resample.SlicedTextures);
        Config.Resample.Padding.BlackListS = ProcessTextureRefs(Config.Resample.Padding.BlackList);

        // Compile blacklist patterns
        static Regex[] ProcessTexturePatterns(List<string> texturePatternStrings) {
            var result = new Regex[texturePatternStrings.Count];
            for (int i = 0; i < texturePatternStrings.Count; ++i) {
                result[i] = ProcessTexturePattern(texturePatternStrings[i]);
            }
            return result;
        }


        Config.Resample.BlacklistPatterns = ProcessTexturePatterns(Config.Resample.Blacklist);
        Config.Resample.GradientBlacklistPatterns = ProcessTexturePatterns(Config.Resample.GradientBlacklist);
    }

    private bool TryAddConsoleCommand(string name, string documentation, Action<string, string[]> callback) {
        try {
            Helper.ConsoleCommands.Add(name, documentation, callback);
            return true;
        }
        catch (Exception ex) {
            Debug.Warning($"Could not register '{name}' for console commands", ex);
            return false;
        }
    }

    private void InitializeEvents() {
        var gameLoop = Helper.Events.GameLoop;

        Helper.Events.Input.ButtonPressed += OnButtonPressed;

        gameLoop.DayEnding += OnDayEnded;
        gameLoop.DayStarted += OnDayStarted;
        // GC after major events
        gameLoop.SaveLoaded += (_, _) => {
            ForceGarbageCollect();
            Garbage.EnterInteractive();
        };
        gameLoop.DayEnding += (_, _) => ForceGarbageCollect();
        gameLoop.ReturnedToTitle += (_, _) => OnTitle();
        gameLoop.GameLaunched += (_, _) => OnGameLaunched();
        gameLoop.SaveCreating += (_, _) => OnSaveStart();
        gameLoop.Saving += (_, _) => OnSaveStart();
        gameLoop.SaveCreated += (_, _) => OnSaveFinish();
        gameLoop.Saved += (_, _) => OnSaveFinish();
        Helper.Events.Display.WindowResized += (_, args) => OnWindowResized(args);
        Helper.Events.Player.Warped += OnWarp;
        Helper.Events.Specialized.LoadStageChanged += (_, args) => {
            switch (args.NewStage) {
                case LoadStage.SaveLoadedBasicInfo:
                case LoadStage.SaveLoadedLocations:
                case LoadStage.Preloaded:
                case LoadStage.ReturningToTitle:
                    Garbage.EnterNonInteractive();
                    break;
            }
        };
        Helper.Events.Display.MenuChanged += OnMenuChanged;
        Helper.Events.Content.AssetsInvalidated += Content_AssetsInvalidated;
    }

    private bool Initialized = false;
    private void Initialize() {
        if (Initialized) {
            ConfigureHarmony(early: false);
            return;
        }

        try {
            Debug.Message(Versioning.StringHeader);

            ConfigureHarmony(early: true);

            InitializeConfig();

            Initialized = true;
        }
        catch {
            // Swallow Exceptions
        }
    }

    [UsedImplicitly]
    public override void Entry(IModHelper help) {
#if !SHIPPING
        ModManifest.UniqueID.AssertEqual(UniqueId);
#endif

        Runtime.CorrectProcessorAffinity();

        Initialize();

        if (Config.ShowIntroMessage && !Config.SkipIntro) {
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Config.ShowIntroMessage = false;
        }

        Serialize.Save(Config.Path);

        foreach (var prefix in new[] { "clearglasses", "cg" }) {
            _ = TryAddConsoleCommand(prefix, "ClearGlasses Commands", ConsoleSupport.Invoke);
        }

        InitializeEvents();

        MemoryMonitor.Start();

        static void SetSystemTarget(XGraphics.RenderTarget2D? target) {
            if (target is null) {
                return;
            }

            target.Meta().IsSystemRenderTarget = true;
        }

        SetSystemTarget(Game1.lightmap);
        SetSystemTarget(Game1.game1.screen);
        SetSystemTarget(Game1.game1.uiScreen);

        // TODO : Iterate deeply with reflection over 'StardewValley' namespace to find any XTexture2D objects sitting around

        RuntimeHelpers.RunClassConstructor(typeof(FileCache).TypeHandle);
        WatchDog.WatchDog.Initialize();
        ClickCrash.Initialize();
    }

    private static class ModUid {
        internal const string DynamicGameAssets = "spacechase0.DynamicGameAssets";
        internal const string ContentPatcher = "Pathoschild.ContentPatcher";
        internal const string ContentPatcherAnimations = "spacechase0.ContentPatcherAnimations";
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs args) {
        if (!Config.ShowIntroMessage) {
            return;
        }

        if (Game1.ticks <= 1) {
            return;
        }

        Configuration.ConfigMenu.Setup.ForceOpen();

        Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
    }

    private void OnWindowResized(WindowResizedEventArgs args) {
        if (args.NewSize == args.OldSize) {
            return;
        }
        Snow.OnWindowResized(args.NewSize);
    }

    private void OnWarp(object? _, WarpedEventArgs args) {
        if (Config.AsyncScaling.FlushSynchronizedTasksOnWarp) {
            SynchronizedTaskScheduler.Instance.FlushPendingTasks();
        }

        ForceGarbageCollectConcurrent();
    }

    private void OnMenuChanged(object? _, MenuChangedEventArgs args) {
        //_ = _;
    }

    private void OnSaveStart() {
        SynchronizedTaskScheduler.Instance.FlushPendingTasks();
        Garbage.EnterNonInteractive();
    }

    private void OnSaveFinish() {
        SynchronizedTaskScheduler.Instance.FlushPendingTasks();
        ForceGarbageCollect();
        Garbage.EnterInteractive();
    }

    private void OnTitle() {
        ForceGarbageCollect();
        Garbage.EnterInteractive();
    }

    internal void OnFirstDraw() {
        Garbage.EnterInteractive();
    }

    private const string UnderTestingMessage = "which is still in testing under SpriteMaster - results may vary";
    private static readonly (string UID, string Name, string Message)[] WarnFrameworks = new (string UID, string Name, string Message)[] {
        (ModUid.ContentPatcherAnimations, "Content Patcher Animations", UnderTestingMessage),
        (ModUid.DynamicGameAssets, "Dynamic Game Assets", UnderTestingMessage),
    };

    [MethodImpl(Runtime.MethodImpl.RunOnce)]
    private void CheckMods() {
        var frameworkedMods = new Dictionary<string, List<IModInfo>>();

        foreach (var mod in Helper.ModRegistry.GetAll()) {
            var manifest = mod.Manifest;

            foreach (var framework in WarnFrameworks) {
                if (
                    !manifest.Dependencies.AnyF(d => d.UniqueID == framework.UID) &&
                    manifest.ContentPackFor?.UniqueID != framework.UID
                ) {
                    continue;
                }

                if (!frameworkedMods.TryGetValue(framework.UID, out var list)) {
                    list = new List<IModInfo>();
                    frameworkedMods.Add(framework.UID, list);
                }
                list.Add(mod);
                break;
            }
        }

        foreach (var modsPair in frameworkedMods) {
            if (modsPair.Value.Count == 0) {
                continue;
            }

            var framework = WarnFrameworks.FirstF(framework => framework.UID == modsPair.Key);

            var sb = new StringBuilder();
            sb.AppendLine($"The following mods have a dependency on {framework.Name} ({framework.UID}), {framework.Message}:");

            foreach (var mod in modsPair.Value) {
                sb.AppendLine($"\t{mod.Manifest.Name} ({mod.Manifest.UniqueID})");
            }

            Debug.Info(sb.ToString());
        }
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly struct WaitWrapper : IDisposable {
        private readonly object Waiter;

        internal WaitWrapper(object waiter) => Waiter = waiter;

        public void Dispose() {
            if (Waiter is IDisposable disposable) {
                disposable.Dispose();
            }
        }

        internal void Wait() {
            switch (Waiter) {
                case Task task:
                    task.Wait();
                    break;
                case ManualCondition condition:
                    condition.Wait();
                    break;
                default:
                    ThrowHelper.ThrowInvalidOperationException(Waiter.GetType().Name);
                    break;
            }
        }
    }

    private void OnGameLaunched() {
        Configuration.ConfigMenu.Setup.Helper = Helper;
        var waiters = new WaitWrapper[] {
            new(Task.Run(CheckMods)),
            new(FileCache.Initialized),
            new(Task.Run(Configuration.ConfigMenu.Setup.Initialize))
        };

        foreach (var waiter in waiters) {
            waiter.Wait();
            waiter.Dispose();
        }

        ForceGarbageCollect();
        ManagedSpriteInstance.ClearTimers();
    }

    private static void OnDayEnded(object? _, DayEndingEventArgs _1) {
        SynchronizedTaskScheduler.Instance.FlushPendingTasks();
    }

    // SMAPI/CP won't do this, so we do. Purge the cached textures for the previous season on a season change.
    private static void OnDayStarted(object? _, DayStartedEventArgs _1) {
        Snow.PopulateDebrisWeatherArray();

        SynchronizedTaskScheduler.Instance.FlushPendingTasks();

        // Do a full GC at the start of each day
        Garbage.Collect(compact: true, blocking: true, background: false);

        var season = Game1.currentSeason;
        if (!season.EqualsInvariantInsensitive(GameState.CurrentSeason)) {
            GameState.CurrentSeason = season;
            SpriteMap.SeasonPurge(season.ToLowerInvariant());

            // And again after purge
            Garbage.Collect(compact: true, blocking: true, background: false);
        }
    }

    private static void ForceGarbageCollect() {
        Garbage.Collect(compact: true, blocking: true, background: false);
    }

    private static void ForceGarbageCollectConcurrent() {
        Garbage.Collect(compact: false, blocking: false, background: true);
    }

    private void ConfigureHarmony(bool early) {
        bool wasInitialized = HarmonyInstance.IsValueCreated;

        var instance = HarmonyInstance.Value;

        // If early initialization hadn't already occurred, do it now.
        if (!early && !wasInitialized) {
            instance.ApplyPatches(early: true);
        }

        instance.ApplyPatches(early);

        Inlining.Reenable();
    }


    private static void OnButtonPressed(object? _, ButtonPressedEventArgs args) {

        if (args.Button == Config.ToggleButton) {
            var keyboardState = Game1.GetKeyboardState();
            var control = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl);
            if (control) {
                Config.ToggledEnable = !Config.ToggledEnable;
            }
            else {
                Config.Resample.ToggledEnable = !Config.Resample.ToggledEnable;
            }
        }
    }

    [EventPriority(EventPriority.High)]
    private static void Content_AssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e) {
        var purged = Metadata.Metadata.Purge(e.Names);
        if (purged.IsEmpty()) {
            Debug.ForceTrace("No textures were purged");
        }
        else {
            Debug.ForceTrace($"Purged: [{string.Join(", ", purged)}]");
        }
    }
}
