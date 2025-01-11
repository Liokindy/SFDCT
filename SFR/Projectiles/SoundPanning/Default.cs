using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using SFD.Projectiles;
using CSound = SFDCT.Game.SoundPatches;
using HarmonyLib;

namespace SFDCT.Projectiles.SoundPanning;

[HarmonyPatch]
internal static class Default
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.DefaultHitObject))]
    private static IEnumerable<CodeInstruction> HitObject(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(33).operand = AccessTools.Method(CSound.typeof_soundHandler, CSound.nameof_soundHandlerPlaySound, CSound.typeof_StringVector2Gameworld);
        code.Insert(31, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(32, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Projectile), nameof(Projectile.Position))));
        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.DefaultHitPlayer))]
    private static IEnumerable<CodeInstruction> HitPlayer(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(22).operand = AccessTools.Method(CSound.typeof_soundHandler, CSound.nameof_soundHandlerPlaySound, CSound.typeof_StringVector2Gameworld);
        code.Insert(20, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(21, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Projectile), nameof(Projectile.Position))));
        return code;
    }
}
