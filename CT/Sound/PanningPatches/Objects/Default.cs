using HarmonyLib;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Sounds;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace SFDCT.Sound.PanningPatches.Objects;

[HarmonyPatch]
internal static class Default
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ObjectData), nameof(ObjectData.OnDestroyGenericCheck))]
    private static IEnumerable<CodeInstruction> OnDestroyGenericCheck(IEnumerable<CodeInstruction> instructions)
    {
        var code = new List<CodeInstruction>(instructions);

        code.Insert(29, new(OpCodes.Ldarg_0));
        code.Insert(30, new(OpCodes.Call, AccessTools.Method(typeof(ObjectData), nameof(ObjectData.GetWorldPosition))));
        code.ElementAt(33).operand = AccessTools.Method(typeof(SoundHandler), nameof(SoundHandler.PlaySound), [typeof(string), typeof(Vector2), typeof(GameWorld)]);

        return code;
    }
}
