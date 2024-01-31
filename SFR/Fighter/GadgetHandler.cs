using System;
using System.Collections.Generic;
using System.Linq;
using SFD;
using SFD.Objects;
using SFD.Weapons;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;
using CSettings = SFDCT.Settings.Values;
using CConst = SFDCT.Misc.Constants;
using SFD.Projectiles;

namespace SFDCT.Fighter;

/// <summary>
///     Here we handle all the HUD or visual effects regarding players, such as dev icons.
/// </summary>
[HarmonyPatch]
internal static class GadgetHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectStreetsweeper), nameof(ObjectStreetsweeper.DrawLazer))]
    private static bool DrawSWLazer(ObjectStreetsweeper __instance, SpriteBatch spriteBatch, Vector2 worldPosition, Vector2 eyeOffset)
    {
        if (__instance == null || __instance.IsDisposed)
        {
            return false;
        }
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
        if (__instance == null || __instance.IsDisposed)
        {
            return true;
        }

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
        if (__instance.CurrentHandgunWeapon != null && __instance.CurrentWeaponDrawn == WeaponItemType.Handgun && __instance.CurrentHandgunWeapon.LazerUpgrade > 0)
        {
            rweapon = __instance.CurrentHandgunWeapon;
        }
        else if (__instance.CurrentRifleWeapon != null && __instance.CurrentWeaponDrawn == WeaponItemType.Rifle && __instance.CurrentRifleWeapon.LazerUpgrade > 0)
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
            if (!lazer_rcR_noise.TunnelCollision && __instance.m_spriteBatch != null && !__instance.m_spriteBatch.IsDisposed)
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
    private static void DrawOmenBar_OnHUD(PlayerHUD __instance, int x, int y, Player player, GameUser user, PlayerStatus playerStatus, SpriteBatch spriteBatch, float elapsed)
    {
        if (player == null || player.IsRemoved || player.IsDisposed)
        {
            return;
        }

        float omenFullness = GetOmenBar(player, out bool omenIsWarning);

        if (omenFullness > 0f)
        {
            Color omenBarColor = CConst.Colors.OmenBar;

            if (omenIsWarning && (int)(GameSFD.LastUpdateNetTimeMS * 0.005f) % 2 == 0)
            {
                omenBarColor = CConst.Colors.OmenFlash;
            }

            SFDCT.Helper.PlayerHUD.DrawBar(spriteBatch, omenBarColor, omenFullness, x + 56, y - 42 + 12, 184, 4);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player), nameof(Player.DrawPlates))]
    private static void DrawOmenBar_OnStatusBar(Player __instance)
    {
        if (!__instance.IsLocal || __instance.IsDead)
        {
            return;
        }
        if ((__instance.DrawStatusInfo & Player.DrawStatusInfoFlags.StatusBars) != Player.DrawStatusInfoFlags.StatusBars)
        {
            return;
        }
        if (__instance.GetCurrentHealthMode() == Player.HealthMode.RocketRideOverHealth)
        {
            return;
        }

        float omenFullness = GetOmenBar(__instance, out bool omenIsWarning);
        if (omenFullness == 0f)
        {
            return;
        }

        float zoomScale = Math.Max(Camera.Zoom * 0.5f, 1f);

        Vector2 vec = Camera.ConvertWorldToScreen(__instance.Position + new Vector2(0, 24f));
        vec.Y -= 11f * zoomScale;

        Rectangle destinationRectangle = new((int)(vec.X - 32 * zoomScale * 0.5f), (int)(vec.Y), (int)(32 * zoomScale), (int)(2f * zoomScale));

        Color omenBarColor = CConst.Colors.OmenBar;
        if (omenIsWarning && (int)(GameSFD.LastUpdateNetTimeMS * 0.005f) % 2 == 0)
        {
            omenBarColor = CConst.Colors.OmenFlash;
        }

        if(__instance.GetCurrentHealthMode() == Player.HealthMode.StrengthBoostOverHealth || __instance.Health.CheckRecentlyModified(2000f) || __instance.Energy.CheckRecentlyModified(2000f))
        {
            // Keep the health bars visible
            destinationRectangle.Y += 1;
            destinationRectangle.Height -= 1;
            omenBarColor.A = 200;
        }
        else
        {
            // Draw outline and background
            Rectangle borderRectangle = destinationRectangle;
            borderRectangle.Inflate((int)(zoomScale), (int)(zoomScale));

            __instance.m_spriteBatch.Draw(Constants.WhitePixel, borderRectangle, Color.Black);
            __instance.m_spriteBatch.Draw(Constants.WhitePixel, destinationRectangle, new Color(32, 32, 32));
        }

        destinationRectangle.Width = (int)(destinationRectangle.Width * omenFullness);
        __instance.m_spriteBatch.Draw(Constants.WhitePixel, destinationRectangle, ColorCorrection.FromXNAToCustom(omenBarColor));
    }

    private static float GetOmenBar(Player player, out bool IsFlashing)
    {
        float omenFullness = 0f;
        IsFlashing = false;
        if (player == null || player.IsDead || player.IsDisposed || player.IsRemoved)
        {
            return omenFullness;
        }

        // Omen bar uses, ordered in low-to-high priority.
        try
        {
            // The player's slowmotion
            if (player.SlowmotionFactor != 1f && player.GameWorld != null)
            {
                List<Slowmotion> sm_l = player.GameWorld.SlowmotionHandler.GetSlowmotions();
                if (sm_l != null && sm_l.Count > 0)
                {
                    if (sm_l.LastOrDefault().PlayerOwnerID == player.ObjectID)
                    {
                        Slowmotion sm = sm_l.Last();
                        float sm_TotalTime = sm.FadeInTime + sm.ActiveTime + sm.FadeOutTime;
                        omenFullness = (sm_TotalTime - sm.Progress) / sm_TotalTime;
                    }
                }
            }

            // Boosts time
            if (player.StrengthBoostActive || player.SpeedBoostActive)
            {
                if (player.TimeSequence.TimeSpeedBoostActive <= 15000f)
                {
                    omenFullness = player.TimeSequence.TimeSpeedBoostActive / 15000f;
                }

                // Prioritize strengthboost
                if (player.TimeSequence.TimeStrengthBoostActive <= 15000f)
                {
                    omenFullness = player.TimeSequence.TimeStrengthBoostActive / 15000f;

                    if (player.GetCurrentHealthMode() == Player.HealthMode.StrengthBoostOverHealth)
                    {
                        IsFlashing = true;
                    }
                }
            }
        
            // Rocket riding time
            if (player.RocketRideProjectileWorldID > 0 && player.RocketRideProjectile != null && player.RocketRideProjectile is ProjectileBazooka)
            {
                // 10s of rocket riding
                omenFullness = (10000f - ((ProjectileBazooka)player.RocketRideProjectile).m_rocketRideTime) * 0.0001f;
                IsFlashing = true;
            }
        }
        catch (Exception) { }

        if (omenFullness > 1)
        {
            omenFullness = 1;
        }
        else if (omenFullness < 0)
        {
            omenFullness = 0;
        }

        return omenFullness;
    }
}