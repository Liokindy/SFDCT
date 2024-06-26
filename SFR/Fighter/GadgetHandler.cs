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
}