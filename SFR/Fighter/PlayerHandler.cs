using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.IO;
using SFD;
using SFD.Code;
using SFD.Projectiles;
using SFD.Objects;
using SFD.Effects;
using SFD.Sounds;
using Box2D.XNA;
using Microsoft.Xna.Framework;
using HarmonyLib;
using CConst = SFDCT.Misc.Constants;
using SFDGameScriptInterface;

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
    /*
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.PlayerWithinDiveReach))]
    private static void Teammate_DiveCheck(ref bool __result, Player __instance, Player player)
    {
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
    */
}