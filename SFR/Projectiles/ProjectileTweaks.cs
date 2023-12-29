using HarmonyLib;
using SFD.Effects;
using SFD.Materials;
using SFD.Sounds;
using SFD;

namespace SFR.Projectiles;

[HarmonyPatch]
internal static class ProjectileTweaks
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFD.Projectiles.Projectile), nameof(SFD.Projectiles.Projectile.DefaultHitObject))]
    private static bool DefaultHitObject(SFD.Projectiles.Projectile __instance, ObjectData objectData, SFD.Projectiles.ProjectileHitEventArgs e)
    {
        if (objectData.GameOwner != GameOwnerEnum.Server)
        {
            // bool inWater = __instance.InWater;
            Material tileFixtureMaterial = objectData.Tile.GetTileFixtureMaterial(__instance.HitFixtureIndex);
            if (tileFixtureMaterial != null)
            {
                if (!__instance.InWater)
                {
                    EffectHandler.PlayEffect(tileFixtureMaterial.Hit.Projectile.HitEffect, __instance.Position, objectData.GameWorld);
                }
                SoundHandler.PlaySound(tileFixtureMaterial.Hit.Projectile.HitSound, __instance.Position, objectData.GameWorld);
                EffectHandler.PlayEffect("BulletHit", __instance.Position, __instance.GameWorld);
            }
        }
        return false;
    }

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
}
