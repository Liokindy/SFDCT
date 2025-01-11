using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using SFD.Projectiles;
using CSound = SFDCT.Game.SoundPatches;
using HarmonyLib;

namespace SFDCT.Projectiles.SoundPanning;

[HarmonyPatch]
internal class ProjectileBowHit
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ProjectileBow), nameof(ProjectileBow.HitPlayer))]
    private static IEnumerable<CodeInstruction> HitPlayer(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(22).operand = AccessTools.Method(CSound.typeof_soundHandler, CSound.nameof_soundHandlerPlaySound, CSound.typeof_StringVector2Gameworld);
        code.ElementAt(35).operand = AccessTools.Method(CSound.typeof_soundHandler, CSound.nameof_soundHandlerPlaySound, CSound.typeof_StringVector2Gameworld);

        code.Insert(20, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(21, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Projectile), nameof(Projectile.Position))));
        code.Insert(33 + 2, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(34 + 2, new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Projectile), nameof(Projectile.Position))));
        return code;
    }
}
