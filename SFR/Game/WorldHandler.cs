using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SFD;
using SFDCT.Helper;
using SFDCT.Sync;
using HarmonyLib;
using Box2D.XNA;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using CSettings = SFDCT.Settings.Values;
using SFD.Sounds;
using SFD.MapEditor;

namespace SFDCT.Game;

/// <summary>
///     This class contain patches that affect all the rounds, such as how the game is supposed to dispose objects.
/// </summary>
[HarmonyPatch]
internal static class WorldHandler
{
    /// <summary>
    ///     For unknown reasons players tempt to crash when joining a game.
    ///     This is caused because a collection is being modified during its iteration.
    ///     Therefore we iterate the collection backwards so it can be modified without throwing an exception.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.FinalizeProperties))]
    private static bool FinalizeProperties(GameWorld __instance)
    {
        __instance.b2_settings.timeStep = 0f;
        __instance.Step(__instance.b2_settings);

        for (int i = __instance.DynamicObjects.Count - 1; i >= 0; i--)
        {
            __instance.DynamicObjects.ElementAt(i).Value.FinalizeProperties();
        }

        for (int i = __instance.StaticObjects.Count - 1; i >= 0; i--)
        {
            __instance.StaticObjects.ElementAt(i).Value.FinalizeProperties();
        }

        return false;
    }

    /// <summary>
    ///     This class will be called at the end of every round.
    ///     Use it to dispose your collections or reset some data.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.DisposeAllObjects))]
    private static void DisposeData()
    {
        // SyncHandler.Attempts.Clear();
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Update))]
    private static void SaturationPatch(GameWorld __instance, float chunkMs, float totalMs, bool isLast, bool isFirst)
    {
        if (!SFD.Program.IsGame || !(__instance.EditMode & !__instance.EditPhysicsRunning) && isLast && __instance.GameOwner != GameOwnerEnum.Server)
        {
            float highestPlayerHealthFullness = 0f;
            for (int i = 0; i < __instance.LocalPlayers.Length; i++)
            {
                Player localPlayer = __instance.LocalPlayers[i];
                if (localPlayer != null && !localPlayer.IsDisposed && !localPlayer.IsDead)
                {
                    highestPlayerHealthFullness = Math.Max(highestPlayerHealthFullness, localPlayer.Health.Fullness);
                }
            }

            if (highestPlayerHealthFullness > 0f)
            {
                if (GameSFD.GUIMode == ShowGUIMode.HideAll)
                {
                    GameSFD.Saturation = 1f;
                }
                else if (highestPlayerHealthFullness < CSettings.GetFloat("LOW_HEALTH_THRESHOLD"))
                {
                    float lowhpFactor = 1f - highestPlayerHealthFullness /  CSettings.GetFloat("LOW_HEALTH_THRESHOLD");

                    if (__instance.m_nextHeartbeatDelay < 400f && highestPlayerHealthFullness < 0.25)
                    {
                        __instance.m_nextHeartbeatDelay += totalMs * Math.Max(1 - highestPlayerHealthFullness / 0.25f, 0.6f);
                    }

                    __instance.m_nextHeartbeatDelay -= totalMs * Math.Max(lowhpFactor, 0.6f);
                    if (__instance.m_nextHeartbeatDelay <= 0f)
                    {
                        Logger.LogDebug(__instance.m_nextHeartbeatDelay);
                        __instance.m_nextHeartbeatDelay = 400f;
                        SoundHandler.PlaySound("Heartbeat", 1f, __instance);
                    }

                    GameSFD.Saturation = 1f - lowhpFactor * CSettings.GetFloat("LOW_HEALTH_SATURATION_FACTOR");
                }
            }
        }
    }
}