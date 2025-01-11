using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Weapons;

namespace SFDCT.Weapons.SoundPanning.Ranged;

[HarmonyPatch]
internal static class Bow
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(WpnBow), nameof(WpnBow.OnSubAnimationEvent))]
    private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);

        code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(17, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));

        return code;
    }
}
