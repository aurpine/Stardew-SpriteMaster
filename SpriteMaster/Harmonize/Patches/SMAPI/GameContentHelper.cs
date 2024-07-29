using SpriteMaster.Metadata;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteMaster.Harmonize.Patches.SMAPI;

internal static class GameContentHelper {
	[Harmonize(
		typeof(StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6.AbigailGameFacade),
		"StardewModdingAPI.Framework.ModHelpers.GameContentHelper",
		"InvalidateCache",
		Harmonize.Fixation.Prefix,
		Harmonize.PriorityLevel.Average,
		critical: false
	)]
	public static bool InvalidateCache(Func<IAssetInfo, bool> predicate) {
		// We have to invalidate all cache because we do not have a way to get an AssetInfo from texture
		Debug.ForceTrace("Invalidating cache");
		Metadata.Metadata.Purge();
		return true;
	}
}
