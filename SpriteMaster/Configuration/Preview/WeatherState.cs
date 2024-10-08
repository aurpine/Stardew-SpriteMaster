﻿using SpriteMaster.Extensions;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static SpriteMaster.Harmonize.Patches.Game.Snow;

namespace SpriteMaster.Configuration.Preview;

[StructLayout(LayoutKind.Auto)]
internal readonly struct WeatherState : IDisposable {
    // SDV
    internal List<WeatherDebris>? DebrisWeather { get; init; }
    internal string Season { get; init; }
    internal RainDrop[]? RainDrops { get; init; }
    internal XVector2 SnowPos { get; init; }
    internal float GlobalWind { get; init; }
    internal float WindGust { get; init; }
    internal bool IsDebrisWeather { get; init; }
    internal bool IsRaining { get; init; }
    internal bool IsSnowing { get; init; }
    internal bool IsLightning { get; init; }

    // SM
    internal SnowState SnowWeatherState { get; init; }

    internal static WeatherState Backup() => new() {
        IsDebrisWeather = Game1.isDebrisWeather,
        DebrisWeather = Game1.debrisWeather is null ? null : new(Game1.debrisWeather),
        Season = Game1.currentSeason,
        GlobalWind = WeatherDebris.globalWind,
        WindGust = Game1.windGust,
        RainDrops = Game1.rainDrops.CloneFast(),
        IsRaining = Game1.isRaining,
        IsSnowing = Game1.isSnowing,
        IsLightning = Game1.isLightning,
        SnowPos = Game1.snowPos,

        SnowWeatherState = SnowState.Backup()
    };

    internal static WeatherState Backup(string? season) => new() {
        IsDebrisWeather = Game1.isDebrisWeather,
        DebrisWeather = Game1.debrisWeather is null ? null : new(Game1.debrisWeather),
        Season = season!,
        GlobalWind = WeatherDebris.globalWind,
        WindGust = Game1.windGust,
        RainDrops = Game1.rainDrops.CloneFast(),
        IsRaining = Game1.isRaining,
        IsSnowing = Game1.isSnowing,
        IsLightning = Game1.isLightning,
        SnowPos = Game1.snowPos,

        SnowWeatherState = SnowState.Backup()
    };

    internal void Restore() {
        Game1.isDebrisWeather = IsDebrisWeather;
        Game1.debrisWeather = DebrisWeather;
        Game1.currentSeason = Season;
        WeatherDebris.globalWind = GlobalWind;
        Game1.windGust = WindGust;
        Game1.rainDrops = RainDrops;
        Game1.isRaining = IsRaining;
        Game1.isSnowing = IsSnowing;
        Game1.isLightning = IsLightning;
        Game1.snowPos = SnowPos;

        SnowWeatherState.Restore();
    }

    public void Dispose() => Restore();
}
