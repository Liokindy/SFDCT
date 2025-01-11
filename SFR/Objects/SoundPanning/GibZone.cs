using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Sounds;
using SFD.Objects;
using System.Linq;

namespace SFDCT.Objects.SoundPanning;

[HarmonyPatch]
internal static class GibZone
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectGibZone), nameof(ObjectGibZone.UpdateObjectBeforeBox2DStep))]
    private static IEnumerable<CodeInstruction> UpdateObjectBeforeBox2DStep(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        // Load the soon-to-be removed player to the stack, instead of the ObjectGibZone
        code.Insert(86, new CodeInstruction(OpCodes.Ldloc_3));
        code.Insert(87, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        code.ElementAt(90).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        return code;
    }
}
