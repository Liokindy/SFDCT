using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Sounds;
using SFD.Objects;
using System.Linq;

namespace SFDCT.Objects.SoundPanning;

[HarmonyPatch]
internal static class StreetsweeperCrate
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectStreetsweeperCrate), nameof(ObjectStreetsweeperCrate.OnActivated))]
    private static IEnumerable<CodeInstruction> OnActivated(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(18).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        code.ElementAt(22).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);

        code.Insert(16, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(17, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));

        code.Insert(22, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(23, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectStreetsweeperCrate), nameof(ObjectStreetsweeperCrate.Open))]
    private static IEnumerable<CodeInstruction> Open(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        // We do this first so we dont take offsets into account
        code.ElementAt(54).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        code.ElementAt(58).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);

        // First, no offset
        code.Insert(52, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(53, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));

        // Second, 2 offset
        code.Insert(58, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(59, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        return code;
    }
}