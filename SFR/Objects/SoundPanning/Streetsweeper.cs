using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Sounds;
using SFD.Objects;
using System.Linq;

namespace SFDCT.Objects.SoundPanning;

[HarmonyPatch]
internal static class Streetsweeper
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectStreetsweeper), nameof(ObjectStreetsweeper.TakeImpactDamage))]
    private static IEnumerable<CodeInstruction> TakeImpactDamage(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.Insert(10, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(11, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        code.ElementAt(14).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectStreetsweeper), nameof(ObjectStreetsweeper.TakeProjectileDamage))]
    private static IEnumerable<CodeInstruction> TakeProjectileDamage(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.Insert(8, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(9, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        code.ElementAt(12).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectStreetsweeper), nameof(ObjectStreetsweeper.UpdateBlinking))]
    private static IEnumerable<CodeInstruction> UpdateBlinking(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        // The original code specifies the volume to be 1f,
        // we'll replace those lines instead of adding 2 new ones
        // since sounds already play at 1f volume by default.
        code.ElementAt(26).opcode = OpCodes.Ldarg_0;
        code.ElementAt(26).operand = null;

        code.Insert(27, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));

        // Offset is 1 instead of 2. 29+1 = 30
        code.ElementAt(30).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        return code;
    }

}
