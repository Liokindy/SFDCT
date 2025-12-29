using HarmonyLib;
using SFD;
using SFD.MP;
using SFD.States;
using SFDCT.Configuration;
using SFDCT.Helper;
using SFDCT.Misc;
using System;
using System.IO;

namespace SFDCT.Bootstrap;

[HarmonyPatch]
internal class TrackHandler
{
    private static bool m_clockTicking = true;
    private const MusicHandler.MusicTrackID m_clockTickingID = (MusicHandler.MusicTrackID)50;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StateLoading), nameof(StateLoading.Load))]
    private static void StateLoading_Load_Prefix(StateLoading __instance)
    {
        if (!__instance.m_isLoaded)
        {
            object obj = __instance.loadResourceObject;
            lock (obj)
            {
                Random rnd = new();
                m_clockTicking = rnd.NextDouble() <= 0.50 && !SFDCTConfig.Get<bool>(CTSettingKey.DisableClockTicking);

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
    private static bool MusicHandler_PlayTitleTrack_Prefix(ref bool __result)
    {
        if (m_clockTicking)
        {
            __result = MusicHandler.PlayTrack(m_clockTickingID, true, 0f);
            return false;
        }

        return true;
    }
}
