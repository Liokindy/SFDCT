using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Sounds;
using SFD.Objects;
using System.Linq;

namespace SFDCT.Objects.SoundPanning;

[HarmonyPatch]
internal static class Plant
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectPlant), nameof(ObjectPlant.OnDestroyObject))]
    private static IEnumerable<CodeInstruction> OnDestroyObject(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.Insert(23, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(24, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        code.ElementAt(27).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        return code;
    }
}
