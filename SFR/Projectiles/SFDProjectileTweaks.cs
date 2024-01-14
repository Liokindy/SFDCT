using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using SFD.Projectiles;
using SFRSound = SFDCT.Game.SoundHandler;
using HarmonyLib;

namespace SFDCT.Projectiles;

/// <summary>
///     Tweaks to the vanilla SFD projectile class
/// </summary>
[HarmonyPatch]
internal static class SFDProjectileTweaks
{
    internal static readonly object get_ProjectilePosition = AccessTools.PropertyGetter(typeof(Projectile), nameof(Projectile.Position));
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.DefaultHitObject))]
    private static IEnumerable<CodeInstruction> DefaultHitObject(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(33).operand = AccessTools.Method(SFRSound.typeof_soundHandler, SFRSound.nameof_soundHandlerPlaySound, SFRSound.typeof_StringVector2Gameworld);
        code.Insert(31, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(32, new CodeInstruction(OpCodes.Call, get_ProjectilePosition));
        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.DefaultHitPlayer))]
    private static IEnumerable<CodeInstruction> DefaultHitPlayer(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(22).operand = AccessTools.Method(SFRSound.typeof_soundHandler, SFRSound.nameof_soundHandlerPlaySound, SFRSound.typeof_StringVector2Gameworld);
        code.Insert(20, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(21, new CodeInstruction(OpCodes.Call, get_ProjectilePosition));
        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ProjectileBow), nameof(ProjectileBow.HitPlayer))]
    private static IEnumerable<CodeInstruction> Bow_HitPlayer(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(22).operand = AccessTools.Method(SFRSound.typeof_soundHandler, SFRSound.nameof_soundHandlerPlaySound, SFRSound.typeof_StringVector2Gameworld);
        code.ElementAt(35).operand = AccessTools.Method(SFRSound.typeof_soundHandler, SFRSound.nameof_soundHandlerPlaySound, SFRSound.typeof_StringVector2Gameworld);

        code.Insert(20, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(21, new CodeInstruction(OpCodes.Call, get_ProjectilePosition));
        code.Insert(33+2, new CodeInstruction(OpCodes.Ldarg_0));
        code.Insert(34+2, new CodeInstruction(OpCodes.Call, get_ProjectilePosition));
        return code;
    }

    /*
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFD.Projectiles.Projectile), nameof(SFD.Projectiles.Projectile.DefaultHitPlayer))]
    private static bool DefaultHitPlayer(SFD.Projectiles.Projectile __instance, Player player, ObjectData playerObjectData)
    {
        if (__instance.GameOwner != GameOwnerEnum.Client)
        {
            player.TakeProjectileDamage(__instance);
            Material material = player.GetPlayerHitMaterial();
            if (material == null)
            {
                material = playerObjectData.Tile.Material;
            }
            SoundHandler.PlaySound(material.Hit.Projectile.HitSound, __instance.Position, __instance.GameWorld);
            EffectHandler.PlayEffect(material.Hit.Projectile.HitEffect, __instance.Position, __instance.GameWorld);
            EffectHandler.PlayEffect("BulletHit", __instance.Position, __instance.GameWorld);
        }
        return false;
    }

    [HarmonyPatch]
    private static class ProjectileBow
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SFD.Projectiles.ProjectileBow), nameof(SFD.Projectiles.ProjectileBow.HitPlayer))]
        private static bool HitPlayer(SFD.Projectiles.ProjectileBow __instance, Player player, ObjectData playerObjectData)
        {
            if (__instance.GameOwner != GameOwnerEnum.Client)
            {
                player.TakeProjectileDamage(__instance);
                Material material = player.GetPlayerHitMaterial();
                if (material == null)
                {
                    material = playerObjectData.Tile.Material;
                }
                SoundHandler.PlaySound(material.Hit.Projectile.HitSound, __instance.Position, __instance.GameWorld);
                EffectHandler.PlayEffect(material.Hit.Projectile.HitEffect, __instance.Position, __instance.GameWorld);
                SoundHandler.PlaySound("MeleeHitSharp", __instance.GameWorld);
            }
            return false;
        }
    }
    */
}
