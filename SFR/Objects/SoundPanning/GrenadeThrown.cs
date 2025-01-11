using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Sounds;
using SFD.Objects;
using System.Linq;

namespace SFDCT.Objects.SoundPanning;

[HarmonyPatch]
internal static class GrenadeThrown
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectGrenadeThrown), nameof(ObjectGrenadeThrown.UpdateObject))]
    private static IEnumerable<CodeInstruction> UpdateObject(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.Insert(37, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(38, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        code.ElementAt(41).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        return code;
    }
}
