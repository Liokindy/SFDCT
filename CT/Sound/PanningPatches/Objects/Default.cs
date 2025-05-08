using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using SFD;
using SFD.Sounds;
using SFDCT.Sound;
using HarmonyLib;

namespace SFDCT.Sound.PanningPatches.Objects;

[HarmonyPatch]
internal static class Default
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectData), nameof(ObjectData.OnDestroyGenericCheck))]
    private static IEnumerable<CodeInstruction> OnDestroyGenericCheck(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.Insert(29, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(30, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        code.ElementAt(33).operand = AccessTools.Method(typeof(SoundHandler), Panning.nameof_SoundHandlerPlaySound, Panning.typeof_String_Vector2_GameWorld);
        return code;
    }
}