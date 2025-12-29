using HarmonyLib;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Projectiles;
using SFD.Sounds;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace SFDCT.Sound.PanningPatches.Projectiles;

[HarmonyPatch]
internal static class Default
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.DefaultHitObject))]
    private static IEnumerable<CodeInstruction> Projectile_DefaultHitObject_SoundPanning(IEnumerable<CodeInstruction> instructions)
    {
        var code = new List<CodeInstruction>(instructions);

        code.ElementAt(33).operand = AccessTools.Method(typeof(SoundHandler), nameof(SoundHandler.PlaySound), [typeof(string), typeof(Vector2), typeof(GameWorld)]);
        code.Insert(31, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(32, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Projectile), nameof(Projectile.Position))));

        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.DefaultHitPlayer))]
    private static IEnumerable<CodeInstruction> Projectile_DefaultHitPlayer_SoundPanning(IEnumerable<CodeInstruction> instructions)
    {
        var code = new List<CodeInstruction>(instructions);

        code.ElementAt(22).operand = AccessTools.Method(typeof(SoundHandler), nameof(SoundHandler.PlaySound), [typeof(string), typeof(Vector2), typeof(GameWorld)]);
        code.Insert(20, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(21, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Projectile), nameof(Projectile.Position))));

        return code;
    }
}
