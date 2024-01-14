using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SFD;
using SFD.States;

namespace SFDCT.Game.Commands;

[HarmonyPatch]
internal static class DebugMouse
{
    public static bool IsEnabled;

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.UpdateDebugMouse), new Type[] { })]
    private static IEnumerable<CodeInstruction> PatchTweaks(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        // code.ElementAt(96).operand = 40f; // MaxForce, default is 150
        // code.ElementAt(100).operand = 0f; // DampingRatio, default is 1
        // code.ElementAt(103).operand = 20f; // FrequencyHz, default is 40

        return code;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameWorld), nameof(GameWorld.Update))]
    private static void Update(float chunkMs, float totalMs, bool isLast, bool isFirst, GameWorld __instance)
    {
        if (isLast && SFD.Program.IsGame && __instance.m_game.CurrentState is not State.EditorTestRun or State.MainMenu && __instance.GameOwner != GameOwnerEnum.Client)
        {
            if (IsEnabled)
            {
                __instance.UpdateDebugMouse();
            }
        }
    }
}

