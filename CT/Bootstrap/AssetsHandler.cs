using System.Collections.Generic;
using SFDCT.Configuration;
using SFDCT.Helper;
using SFDCT.Misc;
using SFD;
using SFD.UserProgression;
using HarmonyLib;
using System.IO;
using SFD.States;
using SFD.MP;
using System;

namespace SFDCT.Bootstrap;

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
        Logger.LogError("Disposing...");

        Logger.LogInfo("Saving IniFile...");
        IniFile.Save();
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
        code.RemoveRange(777, 846 - 777);
        return code;
    }

    /// <summary>
    ///     This removes the lines responsible for sending an Error/Crash Report
    ///     to Mythologic, the game might crash from something only in SFDCT
    ///     and users might send it to Mythologic.
    /// </summary>
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(SFD.Program), nameof(SFD.Program.ShowError))]
    private static IEnumerable<CodeInstruction> StopErrorReport(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        // sfdgameServicesHandler.SFDReportGameError(sfdgameError);
        code.RemoveRange(134, 3);

        // MessageBox.Show("Report successfully sent!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        code.RemoveRange(137, 5);

        return code;
    }

    /// <summary>
    ///     Change window title to Superfighters Deluxe Custom
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), MethodType.Constructor)]
    private static void Init(GameSFD __instance)
    {
        __instance.Window.Title = $"Superfighters Deluxe Custom {Globals.Version.SFDCT}";
    }

    private static bool m_clockTicking = true;
    private const MusicHandler.MusicTrackID m_clockTickingID = (MusicHandler.MusicTrackID)50;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StateLoading), nameof(StateLoading.Load))]
    private static void StateLoadingLoad(StateLoading __instance)
    {
        if (!__instance.m_isLoaded)
        {
            object obj = __instance.loadResourceObject;
            lock (obj)
            {
                Random rnd = new();
                m_clockTicking = rnd.NextDouble() <= 0.10 && !Settings.Get<bool>(SettingKey.DisableClockTicking);

                try
                {
                    if (m_clockTicking)
                    {
                        MusicPlayer.Init(delegate (string information, Exception e)
                        {
                            Logger.LogError(information);
                        });

                        string path = Path.Combine(Globals.Paths.Content, "Data", "Sounds", "Music", "ClockTicking.mp3");
                        
                        MusicHandler.Initialize();
                        MusicHandler.m_trackPaths.Add(m_clockTickingID, path);

                        MusicHandler.PlayTrack(m_clockTickingID, true, 0f);
                        MusicHandler.SetSystemVolume(0.8f, true);
                    }
                }
                catch
                {
                    m_clockTicking = false;
                }
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MusicHandler), nameof(MusicHandler.PlayTitleTrack))]
    private static bool MusicHandlerPlayTitleTrack(ref bool __result)
    {
        if (m_clockTicking)
        {
            // __result = MusicHandler.PlayTrack(m_clockTickingID, true, 0f);
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LanguageHelper), nameof(LanguageHelper.Load))]
    private static void LanguageHelperLoad()
    {
        string filePath = Path.Combine(Globals.Paths.Language, Settings.Get<string>(SettingKey.Language));

        if (!File.Exists(filePath + ".xml"))
        {
            Logger.LogError("LOADING: Failed to find language file at: " + filePath + ".xml");
            Logger.LogError("LOADING: Using default language...");

            filePath = Path.Combine(Globals.Paths.Language, "SFDCT_default");
            Settings.Set<string>(SettingKey.Language, "SFDCT_default");
        }

        filePath = Path.GetFullPath(filePath) + ".xml";

        Logger.LogInfo("LOADING: Language File...");
        LanguageHelper.ReadFile(filePath, LanguageHelper.m_texts, LanguageHelper.m_textHashes);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LanguageFileTranslator), nameof(LanguageFileTranslator.Load))]
    private static void LanguageFileTranslatorLoad()
    {
        string folderPath = Path.GetFullPath(Globals.Paths.Language);

        Logger.LogInfo("LOADING: Language Folder...");
        LanguageFileTranslator.LoadFolder(folderPath);
    }
}