using HarmonyLib;
using SFD.Projectiles;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace SFDCT.Sound.PanningPatches.Projectiles;

[HarmonyPatch]
internal static class Default
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.DefaultHitObject))]
    private static IEnumerable<CodeInstruction> HitObject(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(33).operand = AccessTools.Method(Panning.typeof_SoundHandler, Panning.nameof_SoundHandlerPlaySound, Panning.typeof_String_Vector2_GameWorld);
        code.Insert(31, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(32, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Projectile), nameof(Projectile.Position))));
        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.DefaultHitPlayer))]
    private static IEnumerable<CodeInstruction> HitPlayer(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(22).operand = AccessTools.Method(Panning.typeof_SoundHandler, Panning.nameof_SoundHandlerPlaySound, Panning.typeof_String_Vector2_GameWorld);
        code.Insert(20, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(21, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Projectile), nameof(Projectile.Position))));
        return code;
    }
}