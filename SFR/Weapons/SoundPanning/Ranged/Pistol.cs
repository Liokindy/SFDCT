using HarmonyLib;
using SFD.Weapons;
using SFD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SFDCT.Weapons.SoundPanning.Ranged;

[HarmonyPatch]
internal static class Pistol
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(WpnPistol), nameof(WpnPistol.OnSubAnimationEvent))]
    private static IEnumerable<CodeInstruction> OnSubAnimationEvent(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        code.ElementAt(18).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        code.ElementAt(25).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);

        code.Insert(16, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(17, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));
        code.Insert(23 + 2, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(24 + 2, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));

        return code;
    }
}
