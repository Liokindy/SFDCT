using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Projectiles;
using HarmonyLib;
using SFD.Objects;
using Box2D.XNA;

namespace SFDCT.Fighter;

/// <summary>
///     This class contains all patches regarding players movements, delays etc...
/// </summary>
[HarmonyPatch]
internal static class PlayerHandler
{
    // Keep for future use
    /*
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.CheckLedgeGrab))]
    private static bool CheckLedgeGrab(Player __instance)
    {
        if (__instance.VirtualKeyboardLastMovement is PlayerMovement.Right or PlayerMovement.Left)
        {
            var data = __instance.LedgeGrabData?.ObjectData;
            if (data is ObjectDoor { IsOpen: true })
            {
                __instance.ClearLedgeGrab();
                return false;
            }
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.CanActivateSprint))]
    private static bool ActivateSprint(Player __instance, ref bool __result)
    {
        if (__instance is { CurrentWeaponDrawn: WeaponItemType.Rifle, CurrentRifleWeapon: Barrett, StrengthBoostActive: false })
        {
            __result = false;
            return false;
        }

        return true;
    }
    */
}