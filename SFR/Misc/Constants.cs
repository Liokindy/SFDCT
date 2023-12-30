using HarmonyLib;
using SFD;
using System;
using System.IO;

namespace SFR.Misc;

public static class Constants
{
    public const string SFRVersion = "v.1.3.7d"; // "v.1.0.3b_dev";
    internal static readonly string ServerVersion = "v.1.3.7d";// SFRVersion.Replace("v.1", "v.2");
    internal static readonly Random Random = new();

    internal static readonly string ClientVersion = "v.1.0.0";
    internal static readonly int ConfigurationIniFormat = 1;
    public readonly struct Paths
    {
        public static readonly string Content = Path.Combine(Program.GameDirectory, @"SFR\Content");
        public static readonly string OfficialMaps = Path.Combine(Paths.Content, @"Data\Maps\Official");
        public static readonly string ConfigurationIni = Path.Combine(Paths.Content, @"config.ini");
    }

    /// <summary>
    ///     Panning strength.
    /// </summary>
    public static float SoundPanning_Strength = 1f;
    /// <summary>
    ///     if true, sound-panning is calculated using the
    ///     screen's center.
    ///     if false, sound-panning is calculated using the
    ///     local player's center. (If null
    ///     it will default to the screen's center).
    /// </summary>
    public static bool SoundPanning_IsScreenSpace = false;
    /// <summary>
    ///     Sounds within this distance of the listener will
    ///     not be panned.
    /// </summary>
    public static float SoundPanning_InWorld_Threshold = 64f;
    /// <summary>
    ///     Sounds this distance away from the listener will
    ///     be 100% panned to the corresponding side.
    /// </summary>
    public static float SoundPanning_InWorld_Distance = 360f;
}