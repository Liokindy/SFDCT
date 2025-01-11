using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Sounds;
using SFD.Objects;
using System.Linq;

namespace SFDCT.Objects.SoundPanning;

[HarmonyPatch]
internal static class WoodSupport
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectWoodSupport), nameof(ObjectWoodSupport.OnDestroyObject))]
    private static IEnumerable<CodeInstruction> OnDestroyObject(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(31).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        code.Insert(29, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(30, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        return code;
    }
}
