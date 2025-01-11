using HarmonyLib;
using SFD.Weapons;
using SFD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SFDCT.Weapons.SoundPanning.SoundPanning.Melee;

[HarmonyPatch]
internal static class Chainsaw
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(WpnChainsaw), nameof(WpnChainsaw.Destroyed))]
    private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        code.ElementAt(52).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);

        code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(2, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));

        code.Insert(54, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(55, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));

        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(WpnChainsaw), nameof(WpnChainsaw.OnSubAnimationEvent))]
    private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        code.ElementAt(30).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        code.ElementAt(35).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);

        code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(17, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));

        code.Insert(28 + 2, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(29 + 2, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));

        code.Insert(33 + 4, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(34 + 4, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));

        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(WpnChainsaw), nameof(WpnChainsaw.UpdateExtraMeleeState))]
    private static IEnumerable<CodeInstruction> UpdateExtraMeleeState(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        code.ElementAt(155).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        code.ElementAt(203).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);

        code.Insert(153, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(154, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));

        code.Insert(201 + 2, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(202 + 2, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));

        return code;
    }
}
