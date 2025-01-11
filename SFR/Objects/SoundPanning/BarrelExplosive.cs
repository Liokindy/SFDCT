using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using SFD.Sounds;
using SFD.Objects;
using System.Linq;

namespace SFDCT.Objects.SoundPanning;

[HarmonyPatch]
internal static class BarrelExplosive
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectBarrelExplosive), nameof(ObjectBarrelExplosive.OnDestroyObject))]
    private static IEnumerable<CodeInstruction> OnDestroyObject(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        // Load the object (this.) to the stack
        code.Insert(86, new CodeInstruction(OpCodes.Ldarg_0));

        // Call this.GetWorldPosition()
        code.Insert(87, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));

        // Change PlaySound method used to the one that uses a position,
        // we change element 90 since we added 2 elements, the original
        // instruction is at 88.
        code.ElementAt(90).operand = AccessTools.Method(typeof(SoundHandler), SFDCT.Game.SoundPatches.nameof_soundHandlerPlaySound, SFDCT.Game.SoundPatches.typeof_StringVector2Gameworld);
        return code;
    }
}
