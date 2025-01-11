using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Sounds;
using SFD.Objects;
using System.Linq;

namespace SFDCT.Objects.SoundPanning;

[HarmonyPatch]
internal static class StreetsweeperWreck
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectStreetsweeperWreck), nameof(ObjectStreetsweeperWreck.UpdateObject))]
    private static IEnumerable<CodeInstruction> UpdateObject(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(49).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        code.Insert(47, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(48, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        return code;
    }
}
