using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Weapons;

namespace SFDCT.Weapons.SoundPanning.Ranged;

[HarmonyPatch]
internal static class DarkShotgun
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(WpnDarkShotgun), nameof(WpnDarkShotgun.OnSubAnimationEvent))]
    private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        code.ElementAt(17).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        code.ElementAt(33).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);

        code.Insert(15, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(16, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));
        code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));
        code.Insert(31 + 4, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(32 + 4, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));

        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(WpnDarkShotgun), nameof(WpnDarkShotgun.OnPostFireAnimationEvent))]
    private static IEnumerable<CodeInstruction> OnPostFireAnimationEvent(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        code.ElementAt(51).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        code.ElementAt(59).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);

        code.Insert(49, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(50, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));
        code.Insert(57 + 2, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(58 + 2, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));

        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(WpnDarkShotgun), nameof(WpnDarkShotgun.OnReloadAnimationEvent))]
    private static IEnumerable<CodeInstruction> OnReloadAnimationEvent(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        code.ElementAt(14).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);

        code.Insert(12, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(13, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));

        return code;
    }
}
