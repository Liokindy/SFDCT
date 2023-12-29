using Box2D.XNA;
using HarmonyLib;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Effects;
using SFD.Sounds;
using SFD.Weapons;
using SFR.Objects;
using SFR.Weapons.Rifles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SFR.Fighter;

/// <summary>
///     __instance class contains all patches regarding players movements, delays etc...
/// </summary>
[HarmonyPatch]
internal static class PlayerHandler
{
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

    /// <summary>
    ///     Patches PlayerEmptyBoltActionAnimation so
    ///     it passes a position to PlaySound()
    /// </summary>
    [HarmonyPatch]
    internal static class PlayerEmptyBoltActionAnimation
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SFD.PlayerEmptyBoltActionAnimation), nameof(SFD.PlayerEmptyBoltActionAnimation.OverrideUpperAnimationEnterFrame))]
        private static bool OverrideUpperAnimationEnterFrame(SFD.PlayerEmptyBoltActionAnimation __instance, Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
        {
            if (player.GameOwner != GameOwnerEnum.Server && animEvent == AnimationEvent.EnterFrame)
            {
                if (subAnim.GetCurrentFrameIndex() == 2)
                {
                    SFD.Sounds.SoundHandler.PlaySound("SniperBoltAction1", player.Position, player.GameWorld);
                }
                if (subAnim.GetCurrentFrameIndex() == 3)
                {
                    SFD.Sounds.SoundHandler.PlaySound("SniperBoltAction2", player.Position, player.GameWorld);
                }
            }
            return false;
        }
    }

    /// <summary>
    ///     Patches PlayerEmptyShotgunPumpAnimation so
    ///     it passes a position to PlaySound()
    /// </summary>
    [HarmonyPatch]
    internal static class PlayerEmptyShotgunPumpAnimation
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SFD.PlayerEmptyShotgunPumpAnimation), nameof(SFD.PlayerEmptyShotgunPumpAnimation.OverrideUpperAnimationEnterFrame))]
        private static bool OverrideUpperAnimationEnterFrame(Player player, AnimationEvent animEvent, SubAnimationPlayer subAnim)
        {
            if (player.GameOwner != GameOwnerEnum.Server && animEvent == AnimationEvent.EnterFrame)
            {
                if (subAnim.GetCurrentFrameIndex() == 2)
                {
                    SoundHandler.PlaySound("ShotgunPump1", player.Position, player.GameWorld);
                }
                if (subAnim.GetCurrentFrameIndex() == 3)
                {
                    SoundHandler.PlaySound("ShotgunPump2", player.Position, player.GameWorld);
                }
            }
            return false;
        }
    }
}