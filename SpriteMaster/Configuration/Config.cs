﻿global using SMConfig = SpriteMaster.Configuration.Config;
using LinqFasterer;
using Microsoft.Xna.Framework.Graphics;
using SpriteMaster.Extensions;
using SpriteMaster.Resample;
using SpriteMaster.Types;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Text.RegularExpressions;
using TeximpNet.Compression;

using Root = SpriteMaster;

namespace SpriteMaster.Configuration;

static class Config {
	internal static readonly string ModuleName = typeof(SMConfig).Namespace?.Split('.')?.ElementAtOrDefaultF(0) ?? "SpriteMaster";

	[Attributes.Ignore]
	internal static string Path { get; private set; } = null!;

	[Attributes.Ignore]
	internal static MemoryStream? DefaultConfig = null;

	internal static void SetPath(string path) => Path = path;

	internal const bool IgnoreConfig = false ||
#if DEBUG
		true;
#else
		false;
#endif
	internal const bool SkipIntro = IgnoreConfig;

	[Attributes.Ignore]
	internal static readonly string CurrentVersion = typeof(SMConfig).Assembly.GetCustomAttribute<FullVersionAttribute>()?.Value?.Split('-', 2)?.ElementAtOrDefaultF(0) ??
		throw new BadImageFormatException($"Could not extract version from assembly {typeof(SMConfig).Assembly.FullName ?? typeof(SMConfig).Assembly.ToString()}");

	[Attributes.Ignore]
	internal static readonly Version AssemblyVersionObj = typeof(SMConfig).Assembly.GetName().Version ??
		throw new BadImageFormatException($"Could not extract version from assembly {typeof(SMConfig).Assembly.FullName ?? typeof(SMConfig).Assembly.ToString()}");
	[Attributes.Ignore]
	internal static readonly string AssemblyVersion = AssemblyVersionObj.ToString();

	private enum BuildType {
		Alpha,
		Beta,
		Candidate,
		Final
	}

	[Attributes.Ignore]
	private static string GenerateAssemblyVersionString(int major, int minor, int revision, int build, BuildType type = BuildType.Final, int release = 0) {
		switch (type) {
			case BuildType.Alpha:
				break;
			case BuildType.Beta:
				release += 100;
				break;
			case BuildType.Candidate:
				release += 200;
				break;
			case BuildType.Final:
				release += 300;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(type), type.ToString());
		}

		return $"{major}.{minor}.{revision}.{build + release}";
	}

	[Attributes.GMCMHidden]
	internal static string ConfigVersion = "";
	[Attributes.Ignore]
	internal static string ClearConfigBefore = GenerateAssemblyVersionString(0, 13, 0, 0, BuildType.Final, 0);

	[Attributes.Ignore]
	internal static bool ForcedDisable = false;
	[Attributes.Comment("Should SpriteMaster be enabled?")]
	internal static bool Enabled = true;
	internal static bool IsEnabled => !ForcedDisable && Enabled;
	[Attributes.Comment("Button to toggle SpriteMaster")]
	internal static SButton ToggleButton = SButton.F11;

	[Attributes.Ignore]
	internal static int ClampDimension = BaseMaxTextureDimension; // this is adjustable by the system itself. The user shouldn't be able to touch it.
	internal const int AbsoluteMaxTextureDimension = 16384;
	internal const int BaseMaxTextureDimension = 4096;
	[Attributes.Comment("The preferred maximum texture edge length, if allowed by the hardware")]
	[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
	[Attributes.LimitsInt(min: 1, max: AbsoluteMaxTextureDimension)]
	[Attributes.Advanced]
	internal static int PreferredMaxTextureDimension = 16384;
	internal const bool ClampInvalidBounds = true;
	internal const bool IgnoreUnknownTextures = false;

	[Attributes.Retain]
	[Attributes.GMCMHidden]
	internal static bool ShowIntroMessage = true;

	internal enum Configuration {
		Debug,
		Development,
		Release
	}

	internal const Configuration BuildConfiguration =
#if DEVELOPMENT
			Configuration.Development;
#elif DEBUG
			Configuration.Debug;
#else
			Configuration.Release;
#endif

	internal const bool IsDebug = BuildConfiguration == Configuration.Debug;
	internal const bool IsDevelopment = BuildConfiguration == Configuration.Development;
	internal const bool IsRelease = BuildConfiguration == Configuration.Release;

	[Attributes.Ignore]
	internal static readonly string LocalRootDefault = System.IO.Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		"StardewValley",
		"Mods",
		ModuleName
	);
	internal static string LocalRoot => DataStoreOverride.Length == 0 ? LocalRootDefault : DataStoreOverride;
	[Attributes.Comment("If the data cache is preferred to be elsewhere, it can be set here")]
	[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushFileCache)]
	[Attributes.GMCMHidden]
	internal static string DataStoreOverride = "";

	[Attributes.GMCMHidden]
	[Attributes.Advanced]
	internal static class WatchDog {
		[Attributes.Comment("Should the watchdog be enabled?")]
		internal static bool Enabled = false;
		[Attributes.Comment("What should the default sleep interval be (in milliseconds)?")]
		internal static int DefaultSleepInterval = 5_000;
		[Attributes.Comment("What should the short sleep interval be (in milliseconds)?")]
		internal static int ShortSleepInterval = 500;
		[Attributes.Comment("What should the interrupt interval be (in milliseconds)?")]
		internal static int InterruptInterval = 10_000;
	}

	[Attributes.Advanced]
	internal static class Garbage {
		[Attributes.Comment("Should unowned textures be marked in the garbage collector's statistics?")]
		[Attributes.Advanced]
		internal static bool CollectAccountUnownedTextures = false;
		[Attributes.Comment("Should owned textures be marked in the garbage collector's statistics?")]
		[Attributes.Advanced]
		internal static bool CollectAccountOwnedTextures = false;
		[Attributes.Comment("The amount of free memory required by SM after which it triggers recovery operations")]
		[Attributes.LimitsInt(1, int.MaxValue)]
		[Attributes.Advanced]
		internal static int RequiredFreeMemory = 128;
		[Attributes.Comment("Hysterisis applied to RequiredFreeMemory")]
		[Attributes.LimitsReal(1.01, 10.0)]
		[Attributes.Advanced]
		internal static double RequiredFreeMemoryHysterisis = 1.5;
		[Attributes.Comment("Should sprites containing season names be purged on a seasonal basis?")]
		internal static bool SeasonalPurge = true;
		[Attributes.Comment("What runtime garbage collection latency mode should be set?")]
		internal static GCLatencyMode LatencyMode = GCLatencyMode.SustainedLowLatency;
	}

	[Attributes.Advanced]
	internal static class Debug {
		internal static class Logging {
			internal static LogLevel LogLevel = LogLevel.Trace;
			internal const bool OwnLogFile = true;
#if !SHIPPING
			internal static bool SilenceOtherMods = true;
			internal static string[] SilencedMods = new[] {
				"Farm Type Manager",
				"Quest Framework",
				"AntiSocial NPCs",
				"SMAPI",
				"Json Assets",
				"Content Patcher",
				"Free Love",
				"Mail Framework Mod",
				"Shop Tile Framework",
				"Custom Companions",
				"Farmer Helper",
				"Wind Effects",
				"Multiple Spouse Dialogs"
			};
#endif
		}

		internal static class Sprite {
			internal const bool DumpReference = !IsRelease;
			internal const bool DumpResample = !IsRelease;
		}
	}

	[Attributes.Advanced]
	internal static class DrawState {
		[Attributes.Comment("Enable linear sampling for sprites")]
		internal static bool SetLinear = true;
		[Attributes.Comment("How many MSAA samples should be used?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.ResetDisplay | Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.LimitsInt(1, 16)]
		internal static int MSAASamples = 1;
		[Attributes.Comment("Disable the depth buffer (unused in this game)")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.ResetDisplay)]
		[Attributes.Advanced]
		internal static bool DisableDepthBuffer = false;
		[Attributes.Comment("The default backbuffer format to request")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.ResetDisplay)]
		internal static SurfaceFormat BackbufferFormat = SurfaceFormat.Color;
		[Attributes.Comment("The default HDR backbuffer format to request")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.ResetDisplay)]
		internal static SurfaceFormat BackbufferHDRFormat = SurfaceFormat.Rgba64;
		[Attributes.Comment("Should the system HDR settings be honored?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.ResetDisplay)]
		internal static bool HonorHDRSettings = true;
	}

	[Attributes.Advanced]
	internal static class Performance {
		[Attributes.Comment("Perform a Generation 0 and 1 garbage collection pass every N ticks (if <= 0, disabled)")]
		[Attributes.LimitsInt(0, int.MaxValue)]
		internal static int TransientGCTickCount = 150;
	}

	internal readonly record struct TextureRef(string Texture, Bounds Bounds);

	internal static class Resample {
		[Attributes.Comment("Should resampling be enabled?")]
		internal static bool Enabled = true;
		[Attributes.Comment("Should texture rescaling be enabled?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.Advanced]
		internal static bool Scale = Enabled;
		[Attributes.Comment("What scaling algorithm should be used by default?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		internal static Root.Resample.Scaler Scaler = Root.Resample.Scaler.xBRZ;
		[Attributes.Comment("What scaling algorithm should be used for gradient sprites?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		internal static Root.Resample.Scaler ScalerGradient = Root.Resample.Scaler.None;
		[Attributes.Comment("Should dynamic scaling be used (scaling based upon apparent sprite size)")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.Advanced]
		internal static bool EnableDynamicScale = true;
		[Attributes.Comment("Should we assume that input sprites are gamma corrected?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.Advanced]
		internal static bool AssumeGammaCorrected = true;
		[Attributes.Comment("Maximum scale factor of sprites (clamped to chosen scaler)")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.LimitsInt(1, 6)]
		internal static int MaxScale = 6;
		[Attributes.Comment("Minimum edge length of a sprite to be considered for resampling")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.LimitsInt(1, AbsoluteMaxTextureDimension)]
		[Attributes.Advanced]
		internal static int MinimumTextureDimensions = 1;
		[Attributes.Comment("Should wrapped addressing be enabled for sprite resampling (when analysis suggests it)?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.Advanced]
		internal static bool EnableWrappedAddressing = false;
		[Attributes.Comment("Should resampling be stalled if it is determined that it will cause hitches?")]
		[Attributes.Advanced]
		internal static bool UseFrametimeStalling = true;
		[Attributes.Comment("Should color enhancement/rebalancing be performed?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.Advanced]
		internal static bool UseColorEnhancement = true;
		[Attributes.Comment("Should transparent pixels be premultiplied to prevent a 'halo' effect?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.Advanced]
		internal static bool PremultiplyAlpha = true;
		[Attributes.Comment("Low pass value that should be filtered when reversing premultiplied alpha.")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.LimitsInt(0, 65_535)]
		[Attributes.Advanced]
		internal static int PremultiplicationLowPass = 1024;
		[Attributes.Comment("Use redmean algorithm for perceptual color comparisons?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.Advanced]
		internal static bool UseRedmean = false;
		[Attributes.Comment("What textures are drawn in 'slices' and thus should be special-cased to be resampled as one texture?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.GMCMHidden]
		internal static List<string> SlicedTextures = new() {
			@"LooseSprites\Cursors::0,2000:640,256",
			@"LooseSprites\Cloudy_Ocean_BG",
			@"LooseSprites\Cloudy_Ocean_BG_Night",
			@"LooseSprites\stardewPanorama",
			@"Maps\nightSceneMaru",
			@"Maps\nightSceneMaruTrees",
			@"Maps\sebastianMountainTiles",
			@"Maps\sebastianRideTiles",
			// SVE
			@"Tilesheets\GuntherExpedition2_Shadows",
			@"Tilesheets\Highlands_Fog",
			@"Tilesheets\Highlands_FogBackground",

		};
		[Attributes.Ignore]
		internal static TextureRef[] SlicedTexturesS = Array.Empty<TextureRef>();
		[Attributes.Advanced]
		internal static class BlockMultipleAnalysis {
			[Attributes.Comment("Should sprites be analyzed to see if they are block multiples?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			internal static bool Enabled = true;
			[Attributes.Comment("What threshold should be used for block multiple analysis?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsInt(0, 255)]
			internal static int EqualityThreshold = 1;
			[Attributes.Comment("How many blocks can be different for the test to still pass?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsInt(1, int.MaxValue)]
			internal static int MaxInequality = 1;
		}

		[Attributes.Comment("What textures or spritesheets use 4xblock sizes?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.GMCMHidden]
		internal static List<string> TwoXTextures = new() {
			@"Maps\WoodBuildings" // is _almost_ TwoX.
		};
		[Attributes.Comment("What textures or spritesheets use 4xblock sizes?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.GMCMHidden]
		internal static List<string> FourXTextures = new() {
			@"Characters\Monsters\Crow",
			@"Characters\femaleRival",
			@"Characters\maleRival",
			@"LooseSprites\Bat",
			@"LooseSprites\buildingPlacementTiles",
			@"LooseSprites\chatBox",
			@"LooseSprites\daybg",
			@"LooseSprites\DialogBoxGreen",
			@"LooseSprites\hoverBox",
			@"LooseSprites\nightbg",
			@"LooseSprites\robinAtWork",
			@"LooseSprites\skillTitles",
			@"LooseSprites\textBox",
			@"Maps\busPeople",
			@"Maps\cavedarker",
			@"Maps\FarmhouseTiles",
			@"Maps\GreenHouseInterior",
			@"Maps\MenuTiles",
			@"Maps\MenuTilesUncolored",
			@"Maps\spring_BusStop",
			@"Maps\TownIndoors",
			@"TerrainFeatures\BuffsIcons",
			@"TerrainFeatures\DiggableWall_basic",
			@"TerrainFeatures\DiggableWall_basic_dark",
			@"TerrainFeatures\DiggableWall_frost",
			@"TerrainFeatures\DiggableWall_frost_dark",
			@"TerrainFeatures\DiggableWall_lava",
			@"TerrainFeatures\DiggableWall_lava_dark",
			@"TerrainFeatures\Stalagmite",
			@"TerrainFeatures\Stalagmite_Frost",
			@"TerrainFeatures\Stalagmite_Lava",
			@"TileSheets\Fireball",
			@"TileSheets\rain",
			@"TileSheets\animations"
		};

		[Attributes.Advanced]
		internal static class Analysis {
			[Attributes.Comment("Max color difference to not consider a sprite to be a gradient?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsInt(0, 255)]
			internal static int MaxGradientColorDifference = 38;
			[Attributes.Comment("Minimum different shades required (per channel) for a sprite to be a gradient?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsInt(0, int.MaxValue)]
			internal static int MinimumGradientShades = 2;
			[Attributes.Comment("Use redmean algorithm for perceptual color comparisons?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			internal static bool UseRedmean = true;
		}

		[Attributes.Ignore]
		[Attributes.Advanced]
		internal static class Deposterization {
			[Attributes.Comment("Should deposterization prepass be performed?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			internal const bool PreEnabled = false; // disabled as the system needs more work
			[Attributes.Comment("Should deposterization postpass be performed?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			internal const bool PostEnabled = false; // disabled as the system needs more work
			[Attributes.Comment("Deposterization Color Threshold")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsInt(0, 65_535)]
			internal static int Threshold = 32;
			[Attributes.Comment("Deposterization Block Size")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsInt(1, int.MaxValue)]
			internal static int BlockSize = 1;
			[Attributes.Comment("Default number of passes")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsInt(1, int.MaxValue)]
			internal static int Passes = 2;
			[Attributes.Comment("Use perceptual color for color comparisons?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			internal static bool UsePerceptualColor = true;
			[Attributes.Comment("Use redmean algorithm for perceptual color comparisons?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			internal static bool UseRedmean = false;
		}
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.GMCMHidden]
		internal static readonly List<SurfaceFormat> SupportedFormats = new() {
			SurfaceFormat.Color,
			SurfaceFormat.Dxt5,
			SurfaceFormat.Dxt5SRgb,
			SurfaceFormat.Dxt1,
			SurfaceFormat.Dxt1SRgb,
			SurfaceFormat.Dxt1a,
		};

		[Attributes.Comment("Experimental resample-based recolor support")]
		[Attributes.Advanced]
		internal static class Recolor {
			[Attributes.Comment("Should (experimental) resample-based recoloring be enabled?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			internal static bool Enabled = false;
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsReal(0, double.MaxValue)]
			internal static double RScalar = 0.897642;
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsReal(0, double.MaxValue)]
			internal static double GScalar = 0.998476;
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsReal(0, double.MaxValue)]
			internal static double BScalar = 1.18365;
		}

		[Attributes.Advanced]
		internal static class BlockCompression {
			[Attributes.Comment("Should block compression of sprites be enabled?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			internal static bool Enabled = DevEnabled && (!Runtime.IsMacintosh || MacSupported) && true; // I cannot build a proper libnvtt for OSX presently.
			[Attributes.Ignore]
			private const bool MacSupported = false;
			private const bool DevEnabled = true;
			[Attributes.Comment("What quality level should be used?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			internal static CompressionQuality Quality = CompressionQuality.Highest;
			[Attributes.Comment("What alpha deviation threshold should be applied to determine if a sprite's transparency is smooth or mask-like (determines between bc2 and bc3)?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsInt(0, int.MaxValue)]
			internal static int HardAlphaDeviationThreshold = 7;
		}
		[Attributes.Comment("What spritesheets will absolutely not be resampled or processed?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.GMCMHidden]
		internal static List<string> Blacklist = new() {
			@"LooseSprites\Lighting\",
			@"@^Maps\\.+Mist",
			@"@^Maps\\.+mist",
			@"@^Maps\\.+Shadow",
			@"@^Maps\\.+Shadows",
			@"@^Maps\\.+Fog",
			@"@^Maps\\.+FogBackground",
		};
		[Attributes.Ignore]
		internal static Regex[] BlacklistPatterns = new Regex[0];
		[Attributes.Comment("What spritesheets will absolutely not be treated as gradients?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.GMCMHidden]
		internal static List<string> GradientBlacklist = new() {
			@"TerrainFeatures\hoeDirt"
		};
		[Attributes.Ignore]
		internal static Regex[] GradientBlacklistPatterns = new Regex[0];

		[Attributes.Advanced]
		internal static class Padding {
			[Attributes.Comment("Should padding be applied to sprites to allow resampling to extend beyond the natural sprite boundaries?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			internal static bool Enabled = DevEnabled && true;
			private const bool DevEnabled = true;
			[Attributes.Comment("What is the minimum edge size of a sprite for padding to be enabled?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsInt(1, int.MaxValue)]
			internal static int MinimumSizeTexels = 4;
			[Attributes.Comment("Should unknown (unnamed) sprites be ignored by the padding system?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			internal static bool IgnoreUnknown = false;
			[Attributes.Comment("Should solid edges be padded?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			internal static bool PadSolidEdges = false;

			[Attributes.Comment("What spritesheets should not be padded?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.GMCMHidden]
			internal static List<string> BlackList = new() {
				@"LooseSprites\Cursors::256,308:50,34", // UI borders
			};
			[Attributes.Ignore]
			internal static TextureRef[] BlackListS = Array.Empty<TextureRef>();

			[Attributes.Comment("What spritesheets should have a stricter edge-detection algorithm applied?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.GMCMHidden]
			internal static List<string> StrictList = new() {
				@"LooseSprites\Cursors"
			};
			[Attributes.Comment("What spritesheets should always be padded?")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.GMCMHidden]
			internal static List<string> AlwaysList = new() {
				@"LooseSprites\font_bold",
				@"Characters\Farmer\hairstyles",
				@"Characters\Farmer\hairstyles2",
				@"Characters\Farmer\hats",
				@"Characters\Farmer\pants",
				@"Characters\Farmer\shirts",
				@"TileSheets\weapons",
				@"TileSheets\bushes",
				@"TerrainFeatures\grass",
				@"TileSheets\debris",
				@"TileSheets\animations",
				@"Maps\springobjects",
				@"Maps\summerobjects",
				@"Maps\winterobjects",
				@"Maps\fallobjects",
				@"Buildings\houses",
				@"TileSheets\furniture",
				@"TerrainFeatures\tree1_spring",
				@"TerrainFeatures\tree2_spring",
				@"TerrainFeatures\tree3_spring",
				@"TerrainFeatures\tree1_summer",
				@"TerrainFeatures\tree2_summer",
				@"TerrainFeatures\tree3_summer",
				@"TerrainFeatures\tree1_fall",
				@"TerrainFeatures\tree2_fall",
				@"TerrainFeatures\tree3_fall",
				@"TerrainFeatures\tree1_winter",
				@"TerrainFeatures\tree2_winter",
				@"TerrainFeatures\tree3_winter",
			};
		}

		[Attributes.Comment("Settings common to all scalers")]
		[Attributes.Advanced]
		internal static class Common {
			[Attributes.Comment("The tolerance for colors to be considered equal - [0, 256)")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsInt(0, 255)]
			internal static int EqualColorTolerance = 20;
			[Attributes.Comment("The weight provided to luminance as opposed to chrominance when performing color comparisons")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsReal(0.0, 10.0)]
			internal static double LuminanceWeight = 1.0;
		}

		[Attributes.Advanced]
		internal static class xBRZ {
			[Attributes.Comment("The threshold for a corner-direction to be considered 'dominant'")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsReal(0.0, 10.0)]
			internal static double DominantDirectionThreshold = 4.4;
			[Attributes.Comment("The threshold for a corner-direction to be considered 'steep'")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsReal(0.0, 10.0)]
			internal static double SteepDirectionThreshold = 2.2;
			[Attributes.Comment("Bias towards kernel center applied to corner-direction calculations")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			[Attributes.LimitsReal(0.0, 10.0)]
			internal static double CenterDirectionBias = 3.0;
			[Attributes.Comment("Should gradient block copies be used? (Note: Very Broken)")]
			[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
			internal static bool UseGradientBlockCopy = false;
		}
	}

	[Attributes.Advanced]
	internal static class WrapDetection {
		[Attributes.Comment("Should edge-wrap analysis be enabled?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		internal const bool Enabled = true;
		[Attributes.Comment("What is the threshold percentage of alpha values to be used to determine if it is a wrapping edge?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		[Attributes.LimitsReal(0.0, 1.0)]
		internal static float edgeThreshold = 0.2f;
		[Attributes.Comment("What is the minimum alpha value assumed to be opaque?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushAllInternalCaches)]
		internal static byte alphaThreshold = 1;
	}

	[Attributes.Advanced]
	internal static class AsyncScaling {
		internal const bool Enabled = true;
		[Attributes.Comment("Should asynchronous scaling be enabled for unknown textures?")]
		internal static bool EnabledForUnknownTextures = true;
		[Attributes.Comment("Should synchronous stores always be used?")]
		internal static bool ForceSynchronousStores = !Runtime.Capabilities.AsyncStores;
		[Attributes.Comment("Should synchronous stores be throttled?")]
		internal static bool ThrottledSynchronousLoads = true;
		[Attributes.Comment("Should we fetch and load texture data within the same frame?")]
		internal static bool CanFetchAndLoadSameFrame = true;
		[Attributes.Comment("What is the minimum number of texels in a sprite to be considered for asynchronous scaling?")]
		[Attributes.LimitsInt(0, AbsoluteMaxTextureDimension * AbsoluteMaxTextureDimension)]
		internal static long MinimumSizeTexels = 0;
	}

	[Attributes.Advanced]
	internal static class MemoryCache {
		[Attributes.Comment("Should the memory cache be enabled?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushResidentCache)]
		internal static bool Enabled = DevEnabled && true;
		private const bool DevEnabled = true;
		[Attributes.Comment("Should memory cache elements always be flushed upon update?")]
		internal static bool AlwaysFlush = false;
	}

	[Attributes.Advanced]
	internal static class SuspendedCache {
		[Attributes.Comment("Should the suspended sprite cache be enabled?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushSuspendedSpriteCache)]
		internal static bool Enabled = true;
		[Attributes.Comment("What is the maximum size (in bytes) to store in suspended sprite cache?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushSuspendedSpriteCache)]
		[Attributes.LimitsInt(0, long.MaxValue)]
		internal static long MaxCacheSize = 0x1000_0000L;
		[Attributes.Comment("What is the maximum number of sprites to store in suspended sprite cache?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushSuspendedSpriteCache)]
		[Attributes.LimitsInt(0, int.MaxValue)]
		internal static int MaxCacheCount = 2_000;
	}

	[Attributes.Advanced]
	internal static class SMAPI {
		[Attributes.Comment("Should the experimental SMAPI texture cache patch be enabled?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushTextureCache)]
		internal static bool TextureCacheEnabled = true;
		[Attributes.Comment("Should the PMA texture cache be enabled?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushTextureCache)]
		internal static bool PMATextureCacheEnabled = true;
		[Attributes.Comment("Should the experimental SMAPI texture cache have high memory usage enabled?")]
		[Attributes.Comment("Unrecommended: This results in the game's texture being retained (and thus loaded faster) but doesn't suspend the resampled sprite instances.")]
		internal static bool TextureCacheHighMemoryEnabled = false;
		[Attributes.Comment("Should the ApplyPatch method be patched?")]
		internal static bool ApplyPatchEnabled = true;
		[Attributes.Comment("Should ApplyPatch pin temporary memory?")]
		internal static bool ApplyPatchPinMemory = false;
		[Attributes.Comment("Should 'GetData' be patched to use SM caches?")]
		internal static bool ApplyGetDataPatch = true;
	}

	[Attributes.Advanced]
	internal static class Extras {
		[Attributes.Comment("Should the game have 'fast quitting' enabled?")]
		internal static bool FastQuit = false;
		[Attributes.Comment("Should line drawing be smoothed?")]
		internal static bool SmoothLines = true;
		[Attributes.Comment("Should Harmony patches have inlining re-enabled?")]
		internal static bool HarmonyInlining = false;
		[Attributes.Comment("Should the game's 'parseMasterSchedule' method be fixed and optimized?")]
		internal static bool FixMasterSchedule = true;
		[Attributes.Comment("Should NPC Warp Points code be optimized?")]
		internal static bool OptimizeWarpPoints = true;
		[Attributes.Comment("Should NPCs take true shortest paths?")]
		internal static bool TrueShortestPath = false;
		[Attributes.Comment("Allow NPCs onto the farm?")]
		internal static bool AllowNPCsOnFarm = false;
		[Attributes.Comment("Should the default batch sort be replaced with a stable sort?")]
		internal static bool StableSort = true;
		[Attributes.Comment("Should the game be prevented from going 'unresponsive' during loads?")]
		internal static bool PreventUnresponsive = true;
		[Attributes.Comment("Should the engine's deferred thread task runner be optimized?")]
		internal static bool OptimizeEngineTaskRunner = true;
		internal static class Snow {
			[Attributes.Comment("Should custom snowfall be used during snowstorms?")]
			internal static bool Enabled = true;
			[Attributes.Comment("Minimum Snow Density")]
			[Attributes.LimitsInt(1, int.MaxValue)]
			internal static int MinimumDensity = 1024;
			[Attributes.Comment("Maximum Snow Density")]
			[Attributes.LimitsInt(1, int.MaxValue)]
			internal static int MaximumDensity = 3072;
			[Attributes.Comment("Maximum Snow Rotation Speed")]
			[Attributes.LimitsReal(0.0f, 1.0f)]
			internal static float MaximumRotationSpeed = 1.0f / 60.0f;
			[Attributes.Comment("Maximum Snow Scale")]
			[Attributes.LimitsReal(0.0001f, float.MaxValue)]
			internal static float MaximumScale = 3.0f;
			[Attributes.Comment("Puffersnow Chance")]
			[Attributes.LimitsReal(0.0f, 1.0f)]
			internal static float PuffersnowChance = 0.0f;
		}
		internal static class ModPatches {
			[Attributes.Comment("Patch CustomNPCFixes in order to improve load times?")]
			internal static bool PatchCustomNPCFixes = false;
			[Attributes.Comment("Disable PyTK mitigation for SpriteMaster?")]
			internal static bool DisablePyTKMitigation = true;
		}
	}

	[Attributes.Advanced]
	internal static class FileCache {
		internal const bool Purge = false;
		[Attributes.Comment("Should the file cache be enabled?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushFileCache)]
		internal static bool Enabled = DevEnabled && true;
		private const bool DevEnabled = true;
		internal const int LockRetries = 32;
		internal const int LockSleepMS = 32;
		[Attributes.Comment("What compression algorithm should be used?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushFileCache)]
		internal static Compression.Algorithm Compress = Compression.BestAlgorithm;
		[Attributes.Comment("Should files be compressed regardless of if it would be beneficial?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushFileCache)]
		internal static bool ForceCompress = false;
		[Attributes.Comment("Should system compression (such as NTFS compression) be preferred?")]
		[Attributes.OptionsAttribute(Attributes.OptionsAttribute.Flag.FlushFileCache)]
		internal static bool PreferSystemCompression = false;
		internal const bool Profile = false;
	}
}
