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
    /// <summary>
    ///     Overrides the Streetsweeper drawing method to include
    ///     status bars.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectStreetsweeper), nameof(ObjectStreetsweeper.Draw))]
    private static bool DrawStreetsweeper(ObjectStreetsweeper __instance, SpriteBatch spriteBatch, float ms)
    {
        if (__instance == null || __instance.IsDisposed)
        {
            return false;
        }

        // Damage flashing
        Color baseColor = Color.Gray;
        __instance.GetDrawColor(ref baseColor);

        Vector2 box2dPosition = __instance.Body.GetPosition() + __instance.GameWorld.DrawingBox2DSimulationTimestepOver * __instance.Body.GetLinearVelocity();
        Vector2 worldPosition = Converter.Box2DToWorld(box2dPosition);
        Vector2 screenPosition = Camera.ConvertBox2DToScreen(box2dPosition);

        // Body
        __instance.CurrentAnimation?.Draw(spriteBatch, __instance.Texture, screenPosition, __instance.Body.GetAngle(), SpriteEffects.None, 0.5f);

        // Eye
        Vector2 lookDirVector = new Vector2((float)Math.Cos(__instance.m_lookRotation), -(float)Math.Sin(__instance.m_lookRotation));
        lookDirVector.Normalize();
        lookDirVector *= 1.5f;
        __instance.m_eyePosition = Vector2.SmoothStep(__instance.m_eyePosition, lookDirVector, 0.05f * ms);

        Vector2 eyeTextureOffset = new Vector2(__instance.m_eyeTexture.Width * 0.5f, __instance.m_eyeTexture.Height * 0.5f);
        Color eyeColor = __instance.m_blinking ? ColorCorrection.CreateCustom(Constants.COLORS.LAZER_FULL_STRENGTH) : baseColor;
        __instance.DrawTexture(spriteBatch, worldPosition, __instance.m_eyeTexture, __instance.m_eyePosition, eyeTextureOffset, 0f, eyeColor, SpriteEffects.None, 1f);

        // Lazer and Gun
        ObjectStreetsweeper.AttackStateEnum attackState = __instance.GetAttackState();
        if (attackState is ObjectStreetsweeper.AttackStateEnum.Aiming or ObjectStreetsweeper.AttackStateEnum.Attacking)
        {
            __instance.DrawLazer(spriteBatch, worldPosition, __instance.m_eyePosition);
        }
        __instance.Gun.DrawTexture(spriteBatch, worldPosition, baseColor);
        
        // Health bar and name/plate
        if (__instance.m_showNamePlate)
        {
            Vector2 platesPosition = Camera.ConvertWorldToScreen(worldPosition + new Vector2(0, 16f));
            float zoomScale = MathHelper.Max(Camera.Zoom * 0.5f, 1f);

            // Health and Gun cooldown
            if (__instance.Health.CheckRecentlyModified(2000f) || (__instance.GetAttackState() is ObjectStreetsweeper.AttackStateEnum.Aiming or ObjectStreetsweeper.AttackStateEnum.Attacking))
            {
                Rectangle barRect = new((int)(platesPosition.X - 32 * zoomScale * 0.5f), (int)(platesPosition.Y - 6f * zoomScale), (int)(32 * zoomScale), (int)(2f * zoomScale));
                Rectangle outlineRect = barRect;
                outlineRect.Inflate((int)zoomScale, (int)zoomScale);
                Color healthBarColor = __instance.Health.CheckRecentlyModified(50f) ? Color.White : ColorCorrection.FromXNAToCustom(Constants.COLORS.LIFE_BAR);

                // Black outside outline
                spriteBatch.Draw(Constants.WhitePixel, outlineRect, Color.Black);
                // Inner background
                outlineRect.Inflate(-(int)zoomScale, -(int)zoomScale);
                spriteBatch.Draw(Constants.WhitePixel, outlineRect, new Color(64,64,64));
                // Health
                barRect.Width = (int)(barRect.Width * __instance.Health.Fullness);
                spriteBatch.Draw(Constants.WhitePixel, barRect, healthBarColor);
            }


            Team ownerTeam = __instance.GetOwnerTeam();

            Vector2 namePosition = platesPosition - new Vector2(0f, __instance.m_nameTextSize.Y * zoomScale * 0.75f);
            Color nameColor = __instance.GetTeamTextColor(ownerTeam);
            Constants.DrawString(spriteBatch, Constants.Font1Outline, __instance.m_name, namePosition, nameColor, 0f, __instance.m_nameTextSize * 0.5f, zoomScale * 0.5f, SpriteEffects.None, 0);

            Texture2D teamBadge = Constants.GetTeamIcon(ownerTeam);
            if (teamBadge != null)
            {
                Vector2 badgePosition = new Vector2(platesPosition.X - __instance.m_nameTextSize.X * 0.25f * zoomScale - teamBadge.Width * zoomScale, platesPosition.Y - __instance.m_nameTextSize.Y * zoomScale);
                spriteBatch.Draw(teamBadge, badgePosition, null, Color.Gray, 0f, Vector2.Zero, zoomScale, SpriteEffects.None, 1f);
            }
        }

        return false;
    }

    /// <summary>
    ///     Overrides the lazer drawing method of Streetsweepers
    /// </summary>
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

    /// <summary>
    ///     Overrides the lazer drawing method to include perlin-like noise
    ///     in it's wobbing, making it smoother.
    /// </summary>
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

    /// <summary>
    ///     Fixes the lazer attachment not being drawn in the HUD
    ///     (weapons are directly drawn using their model texture)
    /// </summary>
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

    /// <summary>
    ///     Draw a player omen bar on the HUD
    /// </summary>
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

    /// <summary>
    ///     Draw a player omen bar on their status bars
    /// </summary>
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
            int hHalf = destinationRectangle.Height / 2;
            int hOffset = destinationRectangle.Height - hHalf;

            destinationRectangle.Y += hOffset;
            destinationRectangle.Height -= hHalf;
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

    /// <summary>
    ///     Gets the fullness of a player's omenbar
    /// </summary>
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
            // The player's active slowmotions
            if (player.SlowmotionFactor != 1f && player.GameWorld != null)
            {
                List<Slowmotion> sm_l = player.GameWorld.SlowmotionHandler.GetSlowmotions();
                if (sm_l != null && sm_l.Count > 0)
                {
                    Slowmotion sm;
                    for (int i = 0; i < sm_l.Count; i++)
                    {
                        sm = sm_l.ElementAtOrDefault(i);
                        if (sm.PlayerOwnerID == player.ObjectID)
                        {
                            float sm_TotalTime = sm.FadeInTime + sm.ActiveTime + sm.FadeOutTime;
                            omenFullness = (sm_TotalTime - sm.Progress) / sm_TotalTime;
                        }
                    }
                }
            }

            // Boosts time
            // NOT NETWORKED - TODO: Implement a local prediction
            /*
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
            */

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