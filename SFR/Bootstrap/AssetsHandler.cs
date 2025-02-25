using System.Collections.Generic;
using HarmonyLib;
using SFD;
using SFD.States;
using SFD.UserProgression;
using SFDCT.Helper;
using CConst = SFDCT.Misc.Globals;
using CSettings = SFDCT.Settings.Values;
using CIni = SFDCT.Settings.Config;

namespace SFDCT.Bootstrap;

/// <summary>
///     This is where SFR starts.
///     This class handles and loads all the new textures, music, sounds, tiles, colors etc...
///     This class is also used to tweak some game code on startup, such as window title.
/// </summary>
[HarmonyPatch]
internal static class AssetsHandler
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

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Challenges), nameof(Challenges.Load))]
    private static IEnumerable<CodeInstruction> ItemsLock(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        // At these lines, some items used in the official campaigns and challenges
        // are locked with no real reason. (Except that clothing is weird on FrankenBear/Mech)
        // (Normal bear skin requires extra patching, and therefore is the only skin to not work 
        // on other servers)
        code.RemoveRange(859, 61);
        return code;
    }

    /// <summary>
    ///     Init the configuration ini file
    /// </summary>
    //[HarmonyPostfix]
    //[HarmonyPatch(typeof(Constants), nameof(Constants.Load))]
    //private static void LoadConfigIni()
    //{
    //    CIni.Initialize();
    //}

    /// <summary>
    ///     Change window title to Superfighters Deluxe Custom
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), MethodType.Constructor)]
    private static void Init(GameSFD __instance)
    {
        __instance.Window.Title = $"Superfighters Deluxe Custom {CConst.Version.LABEL}";
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFDConfig), nameof(SFDConfig.SaveConfig))]
    private static void SaveSFDConfig(SFDConfigSaveMode mode)
    {
        lock (SFDConfig.m_saveConfigLock)
        {
            if (mode == SFDConfigSaveMode.HostGameOptions || mode == SFDConfigSaveMode.All)
            {
                SFDConfig.ConfigHandler.UpdateValue("MODERATOR_COMMANDS", string.Join(" ", Constants.MODDERATOR_COMMANDS));
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MusicHandler), nameof(MusicHandler.GetTrackFilePath))]
    private static void GetTitleTrack(ref string __result, MusicHandler.MusicTrackID trackID)
    {
        if (!CSettings.Get<bool>(CSettings.GetKey(CSettings.SettingKey.MainMenuTrackRandom))) { return; }

        if (GameSFD.Handle.CurrentState == State.MainMenu && trackID == MusicHandler.MusicTrackID.MenuTheme)
        {
            List<string> validTracks = [];
            foreach(KeyValuePair<MusicHandler.MusicTrackID, string> kvp in MusicHandler.m_trackPaths)
            {
                validTracks.Add(kvp.Value);
            }

            __result = validTracks[Constants.RANDOM.Next(validTracks.Count)];
            validTracks.Clear();
        }
    }
}