using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SFD;
using SFDCT.Helper;
using SFDCT.Sync;
using HarmonyLib;

namespace SFDCT.Game;

/// <summary>
///     This class contain patches that affect all the rounds, such as how the game is supposed to dispose objects.
/// </summary>
[HarmonyPatch]
internal static class WorldHandler
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Update))]
    private static void UpdateWorld(GameWorld __instance, float chunkMs, float totalMs, bool isLast, bool isFirst)
    {
        if (__instance.GameOwner != GameOwnerEnum.Server)
        {
            LazerDrawing.Update(totalMs);
        }
    }

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
        // Keep for future use
        // SyncHandler.Attempts.Clear();
        Game.LazerDrawing.Dispose();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Update))]
    private static IEnumerable<CodeInstruction> PatchSaturationEffects(IEnumerable<CodeInstruction> instructions)
    {
        // This lowers how much low-hp affects
        // the saturation reduction.
        // --
        // When below 25% health,
        // Saturation = 1 - (1 - hpFullness / 0.25) * [VALUE]
        instructions.ElementAt(770).operand = 0.9f;

        // Saturation when above 25% health,
        // and NOT hiding all HUD elements.
        // instructions.ElementAt(796).operand = 1f;
        return instructions;
    }
}