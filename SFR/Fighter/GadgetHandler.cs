using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using HarmonyLib;
using System.Linq;
using Box2D.XNA;
using SFD.Weapons;
using CSettings = SFDCT.Settings.Values;
using SFD.Objects;

namespace SFDCT.Fighter;

/// <summary>
///     Here we handle all the HUD or visual effects regarding players, such as dev icons.
/// </summary>
[HarmonyPatch]
internal static class GadgetHandler
{
    // Keep for future use
    /*
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.Draw))]
    private static bool DrawPlayer(Player __instance, SpriteBatch spriteBatch, float ms)
    {
        if (__instance.IsNullProfile)
        {
            return false;
        }
        if (__instance.GameWorld == null)
        {
            return false;
        }

        if (__instance.WorldBody != null)
        {
            __instance.UpdatePlayerPositionToBox2DPosition(ms);
        }

        // Show hurt level if we arent a burnt corpse
        if (!__instance.Burned)
        {
            float healthFullness = __instance.Health.Fullness;
            int hurtLevel = (healthFullness <= 0.12f) ? 2 : ((healthFullness <= 0.25f) ? 1 : 0);
            __instance.Equipment.EnsureHurtLevelEquipped(hurtLevel);
        }

        // Grow or shrink smoothly
        if (__instance.m_currentDrawScale != __instance.DrawScale)
        {
            if (__instance.GameWorld.ElapsedTotalRealTime - __instance.CreateTime < 100f)
            {
                __instance.m_currentDrawScale = __instance.DrawScale;
            }
            else if (__instance.m_currentDrawScale < __instance.DrawScale)
            {
                __instance.m_currentDrawScale += 0.0003f * ms;
                if (__instance.m_currentDrawScale > __instance.DrawScale)
                {
                    __instance.m_currentDrawScale = __instance.DrawScale;
                }
            }
            else
            {
                __instance.m_currentDrawScale -= 0.0003f * ms;
                if (__instance.m_currentDrawScale < __instance.DrawScale)
                {
                    __instance.m_currentDrawScale = __instance.DrawScale;
                }
            }
        }

        // Get position with shake
        __instance.Shake.UpdateShake(ms / __instance.GameWorld.SlowmotionHandler.SlowmotionModifier);
        Vector2 drawingPosition = __instance.Shake.ApplyShake(__instance.Position);

        // Draw speedboost's delayed copy
        if (__instance.SpeedBoostActive)
        {
            Vector2 vector2 = drawingPosition - __instance.m_speedBoostDelayedPos;

            float num = vector2.CalcSafeLength();
            if (num > 6f)
            {
                vector2.Normalize();
                if (vector2.IsValid())
                {
                    __instance.m_speedBoostDelayedPos = drawingPosition - vector2 * 5.99f;
                }
            }
            else
            {
                vector2.Normalize();
                if (vector2.IsValid())
                {
                    __instance.m_speedBoostDelayedPos += vector2 * Math.Min(ms / 13f, num);
                }
            }

            Color sbDrawColor = __instance.DrawColor;
            sbDrawColor.A = 40;

            __instance.m_subAnimations[0].Draw(spriteBatch, __instance.m_speedBoostDelayedPos, __instance.m_currentDrawScale, __instance.GetAnimationDirection(), __instance.Rotation + __instance.m_subAnimations[0].Rotation, __instance.Equipment, sbDrawColor, ms);
        }

        if (!__instance.IsDead && GameInfo.LocalPlayerCount <= 1 && __instance.InSameTeam(__instance.GameWorld.GUI_TeamDisplay_LocalGameUserTeam))
        {
            DrawPlayerOutline(__instance, spriteBatch, ms);
        }

        // Draw player
        __instance.m_subAnimations[0].Draw(spriteBatch, drawingPosition, __instance.m_currentDrawScale, __instance.GetAnimationDirection(), __instance.Rotation + __instance.m_subAnimations[0].Rotation, __instance.Equipment, __instance.DrawColor, ms);
            
        return false;
    }
    */
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectStreetsweeper), nameof(ObjectStreetsweeper.DrawLazer))]
    private static bool DrawSWLazer(ObjectStreetsweeper __instance, SpriteBatch spriteBatch, Vector2 worldPosition, Vector2 eyeOffset)
    {
        if (__instance.GetWeaponType() == SFDGameScriptInterface.StreetsweeperWeaponType.Flamethrower)
        { 
            return false;
        }

        __instance.DrawTexture(spriteBatch, worldPosition, Constants.WhitePixel, eyeOffset, new Vector2(0.5f), 0f, ColorCorrection.CreateCustom(Constants.COLORS.LAZER_FULL_STRENGTH), SpriteEffects.None, 1.25f);
        Vector2 lazerStart = __instance.GetWorldPosition() + eyeOffset;
        Vector2 lazerDir = __instance.m_lookDirection;

        float distToCamEdge = Math.Max(Camera.WorldRight - Camera.WorldLeft, Camera.WorldTop - Camera.WorldBottom);
        float accuracyShake = (CSettings.GetBool("LAZER_USE_REAL_ACCURACY") ? 0.075f * 0.18f : 0.002f) * Game.LazerDrawing.GetNoise(__instance.ObjectID);
        SFDMath.RotatePosition(ref lazerDir, accuracyShake, out lazerDir);

        GameWorld.RayCastResult lazer_rcR = __instance.GameWorld.RayCast(lazerStart, lazerDir, 0f, distToCamEdge, new GameWorld.RayCastFixtureCheck(__instance.Gun.LazerRayCastCollision), new GameWorld.RayCastPlayerCheck(__instance.Gun.LazerRayCastPlayerCollision));
        Game.LazerDrawing.DrawLazer(spriteBatch, true, lazer_rcR.StartPosition, lazer_rcR.EndPosition, lazer_rcR.Direction);

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.DrawAim))]
    private static bool DrawPlayerAimLazer(Player __instance, float ms, Player.DrawAimMode aimMode)
    {
        if (__instance.IsRemoved || __instance.InThrowingMode || aimMode != Player.DrawAimMode.Lazer)
        {
            return true;
        }

        // Check if the laser should be drawn
        if (__instance.CurrentAction != PlayerAction.ManualAim && __instance.CurrentAction != PlayerAction.HipFire)
        {
            return true;
        }

        // Get the current weapon
        RWeapon rweapon = null;
        if (__instance.CurrentWeaponDrawn == WeaponItemType.Handgun && __instance.CurrentHandgunWeapon.LazerUpgrade > 0)
        {
            rweapon = __instance.CurrentHandgunWeapon;
        }
        else if (__instance.CurrentWeaponDrawn == WeaponItemType.Rifle && __instance.CurrentRifleWeapon.LazerUpgrade > 0)
        {
            rweapon = __instance.CurrentRifleWeapon;
        }

        if (rweapon != null)
        {
            // Get the weapon muzzle position and direction
            if (!__instance.GetWeaponInformation(Player.WeaponInformationType.LazerPosition, out Vector2 lazerPos, out Vector2 lazerDir))
            {
                return false;
            }

            // Get an estimate distance of the laser to the world's edge
            float distToCamEdge = Math.Max(Camera.WorldRight - Camera.WorldLeft, Camera.WorldTop - Camera.WorldBottom);
            
            float accuracyShake = (CSettings.GetBool("LAZER_USE_REAL_ACCURACY") ? 0.18f * 0.25f * rweapon.Properties.AccuracyDeflection : 0.002f) * Game.LazerDrawing.GetNoise(__instance.ObjectID);
            SFDMath.RotatePosition(ref lazerDir, accuracyShake, out lazerDir);
            GameWorld.RayCastResult lazer_rcR_noise = __instance.GameWorld.RayCast(lazerPos, lazerDir, rweapon.Properties.LazerPosition.X + 4f, distToCamEdge, new GameWorld.RayCastFixtureCheck(__instance.LazerRayCastCollision), new GameWorld.RayCastPlayerCheck(__instance.LazerRayCastPlayerCollision));
            if (!lazer_rcR_noise.TunnelCollision)
            {
                Game.LazerDrawing.DrawLazer(__instance.m_spriteBatch, true, lazer_rcR_noise.StartPosition, lazer_rcR_noise.EndPosition, lazer_rcR_noise.Direction);
            }
            return false;
        }
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerHUD), nameof(PlayerHUD.DrawWeaponSlots))]
    private static void DrawWeaponSlotsLazerAttachment(PlayerHUD __instance, int x, int y, SpriteBatch spriteBatch)
    {
        for (int i = 0; i < 2; i++)
        {
            RWeapon rwpn = (i == 0 ? PlayerHUD.m_currentPlayerGUIInfo.CurrentHandgunWeapon : PlayerHUD.m_currentPlayerGUIInfo.CurrentRifleWeapon);
            if (rwpn == null || rwpn.LazerUpgrade <= 0 || rwpn.Properties.WeaponID == 9)
            {
                // Don't draw the laser attachment for the sniper-rifle
                continue;
            }

            Vector2 position = new Vector2(x + (i == 0 ? 74 : 122), y + 28f);
            Vector2 lazerTexOrigin = new Vector2(WeaponDatabase.m_lazerAttachment.Width * 0.5f, WeaponDatabase.m_lazerAttachment.Height * 0.5f);

            spriteBatch.Draw(WeaponDatabase.m_lazerAttachment, position + rwpn.Properties.LazerPosition, null, Color.White, 0f, lazerTexOrigin, 2f, SpriteEffects.None, 0f);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerHUD), nameof(PlayerHUD.DrawMainPanel))]
    private static void DrawOmenBar(PlayerHUD __instance, int x, int y, Player player, GameUser user, PlayerStatus playerStatus, SpriteBatch spriteBatch, float elapsed)
    {
        float omenFullness = 0f;        
        // The player's slowmotion
        if (player.SlowmotionFactor != 1f && player.GameWorld != null)
        {
            List<Slowmotion> sm_l = player.GameWorld.SlowmotionHandler.GetSlowmotions();
            if (sm_l.Count > 0)
            {
                if (sm_l.LastOrDefault().PlayerOwnerID == player.ObjectID)
                {
                    Slowmotion sm = sm_l.Last();
                    float sm_TotalTime = sm.FadeInTime + sm.ActiveTime + sm.FadeOutTime;
                    omenFullness = (sm_TotalTime - sm.Progress) / sm_TotalTime;
                }
            }
        }

        int healthY = y - 22 - 18 - 2;
        omenFullness = Math.Max(Math.Min(omenFullness, 1f), 0f);
        if (omenFullness > 0f)
        {
            SFDCT.Helper.PlayerHUD.DrawBar(spriteBatch, SFD.Constants.COLORS.ARMOR_BAR, omenFullness, x + 56, healthY + 13, 184, 4);
        }
    }
}