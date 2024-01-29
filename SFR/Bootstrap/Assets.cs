using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using SFD;
using SFD.Code;
using SFD.CollisionGroups;
using SFD.Colors;
using SFD.Effects;
using SFD.GameKeyboard;
using SFD.GUI.Text;
using SFD.GUI;
using SFD.ManageLists;
using SFD.Materials;
using SFD.Parser;
using SFD.PingData;
using SFD.Projectiles;
using SFD.Sounds;
using SFD.States;
using SFD.SteamIntegration;
using SFD.Tiles;
using SFD.UserProgression;
using SFD.Weapons;
using SFDCT.Fighter;
using SFDCT.Helper;
using static System.Net.Mime.MediaTypeNames;
using static SFD.Sounds.SoundHandler;
using CConst = SFDCT.Misc.Constants;
using CSettings = SFDCT.Settings.Values;
using CIni = SFDCT.Misc.ConfigIni;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace SFDCT.Bootstrap;

/// <summary>
///     This is where SFR starts.
///     This class handles and loads all the new textures, music, sounds, tiles, colors etc...
///     This class is also used to tweak some game code on startup, such as window title.
/// </summary>
[HarmonyPatch]
internal static class Assets
{
    /// <summary>
    ///     This method is executed whenever we close the game or it crash.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.OnExiting))]
    private static void Dispose()
    {
        Logger.LogError("Disposing");

        // Settings
        CSettings.CheckOverrides();
        CIni.Save();

        Program.RevertVanillaSFDConfig();
    }

    /// <summary>
    ///     Init the configuration ini file
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.Load))]
    private static void LoadConfigIni()
    {
        CIni.Initialize();
    }

    /// <summary>
    ///     Change window title to Superfighters Deluxe Custom
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), MethodType.Constructor)]
    private static void Init(GameSFD __instance)
    {
        __instance.Window.Title = $"Superfighters Deluxe Custom {CConst.Version.SFDCT}";
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MusicHandler), nameof(MusicHandler.GetTrackFilePath))]
    private static void GetEasterEggTitleTrack(ref string __result, MusicHandler.MusicTrackID trackID)
    {
        if (GameSFD.Handle.CurrentState != State.MainMenu || trackID != MusicHandler.MusicTrackID.MenuTheme)
        {
            return;
        }

        // Shhhhh. Don't tell anyone.
        if (Constants.RANDOM.Next(100) == 0)
        {
            string clockTickingPath = Path.GetFullPath(Path.Combine(CConst.Paths.Content, "ClockTicking_SFDCT_Mix.mp3"));
            if (File.Exists(clockTickingPath))
            {
                __result = clockTickingPath;
            }
        }
    }
}