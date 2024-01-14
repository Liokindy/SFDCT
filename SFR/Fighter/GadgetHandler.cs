using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using HarmonyLib;
using System.Linq;
using Box2D.XNA;

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