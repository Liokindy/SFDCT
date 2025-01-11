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
internal static class Baseball
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(WpnBaseball), nameof(WpnBaseball.Destroyed))]
    private static IEnumerable<CodeInstruction> Destroyed(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        code.ElementAt(3).operand = AccessTools.Method(SFDCT.Game.SoundPatches.typeof_soundHandler, SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        code.Insert(1, new CodeInstruction(OpCodes.Ldarg_1));
        code.Insert(2, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Player), nameof(Player.Position))));

        return code;
    }
}
