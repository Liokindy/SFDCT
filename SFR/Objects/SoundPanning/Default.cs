using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Sounds;
using SFD.Objects;
using System.Linq;

namespace SFDCT.Objects.SoundPanning;

[HarmonyPatch]
internal static class Default
{
    // ObjectData, this fixes most objects getting destroyed.
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectData), nameof(ObjectData.OnDestroyGenericCheck))]
    private static IEnumerable<CodeInstruction> OnDestroyGenericCheck(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.Insert(29, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(30, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        code.ElementAt(33).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        return code;
    }
}
