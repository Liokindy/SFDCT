using Box2D.XNA;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFDCT.Configuration;
using System;

namespace SFDCT.Fighter;

[HarmonyPatch]
internal static class PlayerHandler
{
    //[HarmonyPrefix]
    //[HarmonyPatch(typeof(Player), nameof(Player.DrawAim))]
    //private static bool Player_DrawAim_Prefix_CustomDraw(Player __instance, float ms, Player.DrawAimMode aimMode)
    //{
    //    if (__instance.IsRemoved) return false;

    //    switch (aimMode)
    //    {
    //        case Player.DrawAimMode.Lazer:
    //            if (__instance.InThrowingMode) break;
    //            if (__instance.CurrentAction != PlayerAction.ManualAim && __instance.CurrentAction != PlayerAction.HipFire) break;

    //            RWeapon rangedWeapon = __instance.GetCurrentRangedWeaponInUse();
    //            if (rangedWeapon == null) break;
    //            if (rangedWeapon.LazerUpgrade == 0) break;

    //            Vector2 lazerPosition;
    //            Vector2 lazerDirection;
    //            var lazerTunnel = rangedWeapon.Properties.LazerPosition.X + 4f;

    //            if (!__instance.GetWeaponInformation(Player.WeaponInformationType.LazerPosition, out lazerPosition, out lazerDirection)) break;
    //            if (__instance.CurrentAction == PlayerAction.HipFire && Math.Abs(Vector2.Dot(lazerDirection, Vector2.UnitX)) < 0.7f) break;

    //            SFDMath.RotatePosition(ref lazerDirection, Constants.RANDOM.NextFloat(-0.002f, 0.002f), out lazerDirection);

    //            float distanceToCameraEdge = Camera.GetDistanceToEdge(lazerPosition, lazerDirection);

    //            if (distanceToCameraEdge == -1f) break;
    //            distanceToCameraEdge += 16f;

    //            GameWorld.RayCastResult lazerRayCastResult = __instance.GameWorld.RayCast(lazerPosition, lazerDirection, lazerTunnel, distanceToCameraEdge, new GameWorld.RayCastFixtureCheck(__instance.LazerRayCastCollision), new GameWorld.RayCastPlayerCheck(__instance.LazerRayCastPlayerCollision));
    //            if (lazerRayCastResult.TunnelCollision) break;

    //            __instance.GameWorld.DrawLazer(__instance.m_spriteBatch, __instance.IsLocal, lazerRayCastResult.StartPosition, lazerRayCastResult.EndPosition, lazerRayCastResult.Direction);
    //            break;
    //        case Player.DrawAimMode.ManualAimBox:
    //            if (__instance.CurrentAction != PlayerAction.ManualAim || !__instance.IsLocal) break;

    //            var aimCursorOffset = new Vector2(36, 0);
    //            aimCursorOffset += __instance.GetCurrentRangedWeaponInUse()?.Properties.CursorAimOffset ?? Vector2.Zero;

    //            SFDMath.RotatePosition(ref aimCursorOffset, -__instance.AimAngle, out aimCursorOffset);
    //            aimCursorOffset.X *= __instance.LastDirectionX;

    //            var aimCursorPosition = __instance.Position + aimCursorOffset + __instance.AIM_ARM_OFFSET;
    //            Vector2 muzzlePosition;
    //            Vector2 muzzleDirection;
    //            if (!__instance.GetWeaponInformation(Player.WeaponInformationType.MuzzlePosition, out muzzlePosition, out muzzleDirection)) break;

    //            var directionToAimCursor = aimCursorPosition - muzzlePosition;
    //            var distanceToAimCursor = directionToAimCursor.CalcSafeLength();
    //            directionToAimCursor.Normalize();

    //            GameWorld.RayCastResult aimCursorRayCastResult = __instance.GameWorld.RayCast(muzzlePosition, directionToAimCursor, 0f, distanceToAimCursor, new GameWorld.RayCastFixtureCheck(__instance.LazerRayCastCollision), new GameWorld.RayCastPlayerCheck(__instance.LazerRayCastPlayerCollision));
    //            if (!aimCursorRayCastResult.TunnelCollision)
    //            {
    //                aimCursorPosition = aimCursorRayCastResult.EndPosition;

    //                Camera.ConvertWorldToScreen(aimCursorPosition.X, aimCursorPosition.Y, out aimCursorPosition.X, out aimCursorPosition.Y);
    //                var crosshairOffset = new Vector2(Player.m_textureCrosshair.Width, Player.m_textureCrosshair.Height) * 0.5f;
    //                var crosshairScale = Math.Max(Camera.Zoom / 2f, 1f);

    //                __instance.m_spriteBatch.Draw(Player.m_textureCrosshair, aimCursorPosition, null, Color.Gray, 0f, crosshairOffset, crosshairScale, SpriteEffects.None, 0f);
    //            }
    //            break;
    //    }
    //    return false;
    //}

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.Draw))]
    private static bool Player_Draw_Prefix_CustomDraw(Player __instance, SpriteBatch spriteBatch, float ms)
    {
        if (__instance.IsNullProfile) return false;
        if (__instance.WorldBody != null) __instance.UpdatePlayerPositionToBox2DPosition(ms);

        var position = __instance.Position;
        var slowMotionModifier = __instance.GameWorld.SlowmotionHandler.SlowmotionModifier;

        __instance.Shake.UpdateShake(ms / slowMotionModifier);
        position = __instance.Shake.ApplyShake(position);

        int hurtLevel = 0;

        if (!__instance.Burned)
        {
            var healthFullness = __instance.Health.Fullness;

            if (healthFullness <= SFDCTConfig.Get<float>(CTSettingKey.LowHealthHurtLevel2Threshold))
            {
                hurtLevel = 2;
            }
            else if (healthFullness <= SFDCTConfig.Get<float>(CTSettingKey.LowHealthHurtLevel1Threshold))
            {
                hurtLevel = 1;
            }
        }

        __instance.Equipment.EnsureHurtLevelEquipped(hurtLevel);

        var desiredScale = __instance.DrawScale;
        var currentScale = __instance.m_currentDrawScale;

        if (currentScale != desiredScale)
        {
            var lifeTime = __instance.GameWorld.ElapsedTotalRealTime - __instance.CreateTime;

            if (lifeTime < 100f)
            {
                currentScale = __instance.DrawScale;
            }
            else
            {
                var scaleSpeed = 0.0003f;

                if (currentScale < desiredScale)
                {
                    currentScale += scaleSpeed * ms;

                    if (currentScale > desiredScale) currentScale = desiredScale;
                }
                else
                {
                    currentScale -= scaleSpeed * ms;

                    if (currentScale < desiredScale) currentScale = desiredScale;
                }
            }

            __instance.m_currentDrawScale = currentScale;
        }

        if (__instance.SpeedBoostActive)
        {
            var positionDifference = position - __instance.m_speedBoostDelayedPos;
            var direction = positionDifference;
            direction.Normalize();

            if (direction.IsValid())
            {
                var length = positionDifference.CalcSafeLength();

                if (length > 6f)
                {
                    __instance.m_speedBoostDelayedPos = position - direction * 5.99f;
                }
                else
                {
                    __instance.m_speedBoostDelayedPos += direction * Math.Min(ms / 13f, length);
                }
            }

            var ghostColor = __instance.DrawColor;
            ghostColor.A = 40;

            __instance.m_subAnimations[0].Draw(spriteBatch, __instance.m_speedBoostDelayedPos, __instance.m_currentDrawScale, __instance.GetAnimationDirection(), __instance.Rotation + __instance.m_subAnimations[0].Rotation, __instance.Equipment, ghostColor, ms);
        }

        __instance.m_subAnimations[0].Draw(spriteBatch, position, __instance.m_currentDrawScale, __instance.GetAnimationDirection(), __instance.Rotation + __instance.m_subAnimations[0].Rotation, __instance.Equipment, __instance.DrawColor, ms);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.DrawPlates))]
    private static bool Player_DrawPlates_Prefix_CustomDraw(Player __instance, float ms)
    {
        if (__instance.IsDead) return false;

        var zoom = Math.Max(Camera.Zoom * 0.5f, 1f);
        var nameTagPosition = Camera.ConvertWorldToScreen(__instance.Position + Vector2.UnitY * 24f);
        var statusBarsPosition = nameTagPosition - Vector2.UnitY * 11f * zoom;

        // Name-Tag, Team Icon
        if ((__instance.DrawStatusInfo & Player.DrawStatusInfoFlags.Name) == Player.DrawStatusInfoFlags.Name)
        {
            var namePosition = new Vector2(nameTagPosition.X, nameTagPosition.Y - 0.75f * __instance.m_nameTextSize.Y * zoom);
            var nameColor = __instance.GetTeamTextColor();

            Constants.DrawString(__instance.m_spriteBatch, Constants.Font1Outline, __instance.Name, namePosition, nameColor, 0f, __instance.m_nameTextSize * 0.5f, zoom * 0.5f, SpriteEffects.None, 0);

            Texture2D teamIcon = Constants.GetTeamIcon(__instance.m_currentTeam);
            if (teamIcon != null)
            {
                var teamIconPosition = new Vector2(nameTagPosition.X - __instance.m_nameTextSize.X * 0.25f * zoom - (float)teamIcon.Width * zoom, nameTagPosition.Y - __instance.m_nameTextSize.Y * zoom);

                __instance.m_spriteBatch.Draw(teamIcon, teamIconPosition, null, Color.Gray, 0f, Vector2.Zero, zoom, SpriteEffects.None, 1f);
            }
        }

        // Chat Icon
        if (__instance.ChatActive)
        {
            var chatIconTime = 250f;
            var chatIconFrames = 4;

            __instance.m_chatIconTimer += ms;
            if (__instance.m_chatIconTimer > chatIconTime)
            {
                __instance.m_chatIconFrame = (__instance.m_chatIconFrame + 1) % chatIconFrames;
                __instance.m_chatIconTimer -= chatIconTime;
            }

            var chatIcon = Constants.ChatIcon;
            var chatIconColor = ColorCorrection.FromXNAToCustom(Constants.COLORS.CHAT_ICON);
            var chatIconPosition = new Vector2(nameTagPosition.X + __instance.m_nameTextSize.X * 0.25f * zoom, nameTagPosition.Y - __instance.m_nameTextSize.Y * zoom);
            var chatIconUV = new Rectangle(1 + __instance.m_chatIconFrame * (chatIcon.Width / 4), 1, (chatIcon.Width - 2) / 4, chatIcon.Height - 2);

            __instance.m_spriteBatch.Draw(Constants.ChatIcon, chatIconPosition, chatIconUV, chatIconColor, 0f, Vector2.Zero, zoom, SpriteEffects.None, 1f);
        }

        // Status Bars
        if ((__instance.DrawStatusInfo & Player.DrawStatusInfoFlags.StatusBars) == Player.DrawStatusInfoFlags.StatusBars)
        {
            var healthMeter = __instance.Health;
            var healthColor = Constants.COLORS.LIFE_BAR;
            var energyColor = Constants.COLORS.ENERGY_BAR;

            var showStatusBars = __instance.Health.CheckRecentlyModified(2000f) || __instance.Energy.CheckRecentlyModified(2000f);

            switch (__instance.GetCurrentHealthMode())
            {
                case Player.HealthMode.StrengthBoostOverHealth:
                case Player.HealthMode.RocketRideOverHealth:
                    // Blink over time, always show
                    healthMeter = __instance.OverHealth;
                    healthColor = ((int)(GameSFD.LastUpdateNetTimeMS / 200f) % 2 == 0) ? Constants.COLORS.LIFE_BAR_OVERHEALTH_A : Constants.COLORS.LIFE_BAR_OVERHEALTH_B;

                    showStatusBars = true;
                    break;
            }

            if (showStatusBars)
            {
                var barWidth = 32f * zoom;
                var barHeight = 2f * zoom;
                var barCount = 2;

                var barRectangle = new Rectangle((int)(statusBarsPosition.X - barWidth / 2f), (int)statusBarsPosition.Y, (int)barWidth, (int)barHeight);
                var barsBackgroundRectangle = new Rectangle(barRectangle.X, barRectangle.Y, barRectangle.Width, barRectangle.Height * barCount);

                var barsOutlineThickness = 1f * zoom;
                var barsOutlineRectangle = new Rectangle(barRectangle.X, barRectangle.Y, barRectangle.Width, barRectangle.Height * barCount);
                barsOutlineRectangle.Inflate((int)barsOutlineThickness, (int)barsOutlineThickness);

                __instance.m_spriteBatch.Draw(Constants.WhitePixel, barsOutlineRectangle, Color.Black);

                if (healthMeter.CheckRecentlyModified(50f)) healthColor = Color.White;

                var originalWidth = barRectangle.Width;

                // Gray Background
                __instance.m_spriteBatch.Draw(Constants.WhitePixel, barsBackgroundRectangle, new Color(64, 64, 64));

                // Health
                barRectangle.Width = (int)(originalWidth * healthMeter.Fullness);
                __instance.m_spriteBatch.Draw(Constants.WhitePixel, barRectangle, ColorCorrection.FromXNAToCustom(healthColor));

                barRectangle.Y += barRectangle.Height;

                // Energy
                barRectangle.Width = (int)(originalWidth * __instance.Energy.Fullness);
                __instance.m_spriteBatch.Draw(Constants.WhitePixel, barRectangle, ColorCorrection.FromXNAToCustom(energyColor));
            }
        }

        return false;
    }
}
