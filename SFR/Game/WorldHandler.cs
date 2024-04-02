using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SFD;
using SFDCT.Helper;
using SFDCT.Sync;
using HarmonyLib;
using Box2D.XNA;

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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.GetStartupPlayerFlashTime))]
    private static bool GetStartupPlayerFlashTime(GameWorld __instance, ref float __result)
    {
        // 1.25s without startup sequence, 2.25s by default
        // 1.5s without startup or iris, 2.55s with both

        __result = 1500f;

        // The transition may cover the player for some time on startup
        if (__instance.ObjectWorldData.StartupIrisSwipeEnabled)
        {
            __result += 300f;
        }

        // The GET READY... FIGHT! may cover the player and/or distract the user
        if (__instance.ObjectWorldData.StartupSequenceEnabled)
        {
            __result += 750f;
        }

        return false;
    }
}