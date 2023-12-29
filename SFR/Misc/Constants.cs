using HarmonyLib;
using SFD;
using System;

namespace SFR.Misc;

public static class Constants
{
    public const string SFRVersion = "v.1.3.7d"; // "v.1.0.3b_dev";
    internal static readonly string ServerVersion = "v.1.3.7d";// SFRVersion.Replace("v.1", "v.2");
    internal static readonly Random Random = new();

    /// <summary>
    ///     Controls the strength of screen-space panning for sounds
    /// </summary>
    public static float SoundPanningStrength = 1.75f; // 1 = Default, 0-1 Lower strength, >1 Increases strength
}