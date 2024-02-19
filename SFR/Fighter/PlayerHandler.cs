using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Projectiles;
using SFD.Objects;
using SFD.Effects;
using SFD.Sounds;
using Box2D.XNA;
using HarmonyLib;

namespace SFDCT.Fighter;

/// <summary>
///     This class contains all patches regarding players movements, delays etc...
/// </summary>
[HarmonyPatch]
internal static class PlayerHandler
{
    /// <summary>
    ///     Teammates cannot catch other teammates in dives, unless
    ///     the other teammate was caught in a dive/grab
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.PlayerWithinDiveReach))]
    private static void Teammate_DiveCheck(ref bool __result, Player __instance, Player player)
    {
        return;

        if (__instance.GameOwner == GameOwnerEnum.Client)
        {
            return;
        }

        if (__result)
        {
            if (__instance.InSameTeam(player) && !player.IsCaughtByPlayer)
            {
                __result = false;
            }
        }
    }

    /// <summary>
    ///     NOT IMPLEMENTED.
    ///     
    ///     Vanilla client predict the projectile hitting the player and
    ///     removes it from the client realm. May get confusing in online
    ///     situations with latency.
    /// </summary>
    /*
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.TestProjectileHit))]
    private static void Teammate_ProjectileHit(ref bool __result, Player __instance, Projectile projectile)
    {
        if (__instance.GameOwner == GameOwnerEnum.Client)
        {
            return;
        }

        // Dodged the projectile by other means
        if (!__result)
        {
            return;
        }

        // Hit corpses as normal
        if (__instance.IsDead)
        {
            return;
        }

        if (projectile != null && projectile.PlayerOwner != null && !projectile.PlayerOwner.IsDisposed && !projectile.PlayerOwner.IsRemoved)
        {
            if (projectile.TotalDistanceTraveled <= 20 && projectile.PlayerOwner.InSameTeam(__instance) && !__instance.Falling)
            {
                __result = false;
                return;
            }
        }
    }
    */
}