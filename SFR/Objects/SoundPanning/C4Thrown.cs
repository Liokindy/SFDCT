using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Sounds;
using SFD.Objects;
using System.Linq;

namespace SFDCT.Objects.SoundPanning;

[HarmonyPatch]
internal static class C4Thrown
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectC4Thrown), nameof(ObjectC4Thrown.Initialize))]
    private static IEnumerable<CodeInstruction> Initialize(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.Insert(47, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(48, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        code.ElementAt(51).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectC4Thrown), nameof(ObjectC4Thrown.PropertyValueChanged))]
    private static IEnumerable<CodeInstruction> PropertyValueChanged(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.Insert(31, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(32, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        code.ElementAt(35).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectC4Thrown), nameof(ObjectC4Thrown.UpdateObject))]
    private static IEnumerable<CodeInstruction> UpdateObject(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.Insert(56, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(57, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        code.ElementAt(60).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        return code;
    }
}
