using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using HarmonyLib;
using System.Linq;
using Box2D.XNA;

namespace SFR.Fighter;

/// <summary>
///     Here we handle all the HUD or visual effects regarding players, such as dev icons.
/// </summary>
[HarmonyPatch]
internal static class GadgetHandler
{
    /*
    private static DevIcon _devIcon;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectStreetsweeper), nameof(ObjectStreetsweeper.GetOwnerTeam))]
    private static bool FixDroneTeam(ref Team __result)
    {
        __result = _devIcon.Team;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.GetTeamIcon))]
    private static bool DrawDevIcon(Team team, ref Texture2D __result)
    {
        int num = (int)team;
        if (num == -1 && _devIcon.Account != null)
        {
            __result = NameIconHandler.GetDeveloperIcon(_devIcon.Account);
            return false;
        }

        return true;
    }

    internal static Team GetActualTeam(this Player player)
    {
        if (!player.IsBot)
        {
            var user = player.GetGameUser();
            if (user != null && _devIcon.Account == user.Account)
            {
                return _devIcon.Team;
            }
        }

        return player.CurrentTeam;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.DrawPlates))]
    private static bool DrawPlates(Player __instance)
    {
        if (__instance is { IsBot: false, IsDead: false })
        {
            var user = __instance.GetGameUser();
            if (user != null && NameIconHandler.IsDeveloper(user.Account))
            {
                if (_devIcon.Account == null)
                {
                    _devIcon = new DevIcon(__instance.CurrentTeam, user.Account);
                }
                else if (_devIcon.Account != user.Account)
                {
                    _devIcon.Account = user.Account;
                }
                else if (__instance.CurrentTeam >= 0)
                {
                    _devIcon.Team = __instance.CurrentTeam;
                }

                __instance.m_currentTeam = (Team)(-1);
            }
        }

        return true;
    }
    */

    /*
    private static void DrawPlayerOutline(Player player, SpriteBatch spriteBatch, float ms)
    {
        int teamCount = player.GameWorld.Players.Where(p => p.InSameTeam(player.GameWorld.GUI_TeamDisplay_LocalGameUserTeam)).Count();
        if (teamCount > 0)
        {
            Equipment equipmentOnlyClothingItems = player.Equipment;
            // Skip rendering hurt level
            if (equipmentOnlyClothingItems.m_equippedItems[9] != null)
            {
                equipmentOnlyClothingItems.Unequip(9);
            }

            Color outlineCol = SFR.Helper.PlayerHUD.GetPlayerTeamOutlineColor(player);
            outlineCol.A = 128; // This give an antialiasing-like look
            float outlineOffset = 2f / Camera.Zoom;

            player.m_subAnimations[0].Draw(spriteBatch, player.Position + new Vector2(-outlineOffset, outlineOffset), player.m_currentDrawScale, player.GetAnimationDirection(), player.Rotation + player.m_subAnimations[0].Rotation, equipmentOnlyClothingItems, outlineCol, ms);
            player.m_subAnimations[0].Draw(spriteBatch, player.Position + new Vector2(outlineOffset, outlineOffset), player.m_currentDrawScale, player.GetAnimationDirection(), player.Rotation + player.m_subAnimations[0].Rotation, equipmentOnlyClothingItems, outlineCol, ms);
            player.m_subAnimations[0].Draw(spriteBatch, player.Position + new Vector2(outlineOffset, -outlineOffset), player.m_currentDrawScale, player.GetAnimationDirection(), player.Rotation + player.m_subAnimations[0].Rotation, equipmentOnlyClothingItems, outlineCol, ms);
            player.m_subAnimations[0].Draw(spriteBatch, player.Position + new Vector2(-outlineOffset, -outlineOffset), player.m_currentDrawScale, player.GetAnimationDirection(), player.Rotation + player.m_subAnimations[0].Rotation, equipmentOnlyClothingItems, outlineCol, ms);
        }
    }

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

    // - TODO: Somehow store more data in the PlayerGUIInformation class
    //  so we can store information for more data
    // - TODO: Implement SFR's extendedplayer class
    //  so we can store information for data's (last) top limit
    //  and not hard code the upper limit
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
            SFR.Helper.PlayerHUD.DrawBar(spriteBatch, SFD.Constants.COLORS.ARMOR_BAR, omenFullness, x + 56, healthY + 13, 184, 4);
        }
    }
}

/*
internal struct DevIcon
{
    internal Team Team;
    internal string Account;

    internal DevIcon(Team team, string account)
    {
        Team = team;
        Account = account;
    }
}
*/