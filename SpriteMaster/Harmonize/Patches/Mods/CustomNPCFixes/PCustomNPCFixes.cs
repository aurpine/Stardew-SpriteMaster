﻿using SpriteMaster.Configuration;
using StardewValley;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpriteMaster.Harmonize.Patches.Mods.CustomNPCFixes;

internal static class PCustomNPCFixes {
    [Harmonize(
        "CustomNPCFixes.Mod",
        "FixSchedules",
        fixation: Harmonize.Fixation.Prefix,
        priority: Harmonize.PriorityLevel.Last,
        critical: false,
        forMod: "spacechase0.CustomNPCFixes"
    )]
    public static bool FixSchedules(StardewModdingAPI.Mod __instance) {
        if (!Config.IsEnabled || !Config.Extras.ModPatches.PatchCustomNPCFixes) {
            return true;
        }

        List<NPC?> allCharacters = Utility.getAllCharacters()!;
        var processedSet = new ConcurrentDictionary<NPC, byte>();

        Parallel.ForEach(allCharacters, npc => {
            if (npc is null || npc.Schedule is not null) {
                return;
            }

            if (!processedSet.TryAdd(npc, 0)) {
                return;
            }

            try {
                npc.TryLoadSchedule();
                npc.checkSchedule(Game1.timeOfDay);
            }
            catch (Exception ex) {
                Debug.Warning($"CustomNPCFixes Override: Exception processing schedule for NPC '{npc.Name}'", ex);
            }
        });

        return false;
    }
}
