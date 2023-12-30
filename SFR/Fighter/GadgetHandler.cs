using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.Objects;
using HarmonyLib;
using Box2D.XNA;

namespace SFR.Fighter;

/// <summary>
///     Here we handle all the HUD or visual effects regarding players, such as dev icons.
/// </summary>
[HarmonyPatch]
internal static class GadgetHandler
{
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

    // Draw local players at full saturation
    /*
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.Draw))]
    private static bool DrawPlayer(Player __instance, SpriteBatch spriteBatch, float ms)
    {
        if (__instance.IsNullProfile)
        {
            return false;
        }

        if (__instance.WorldBody != null)
        {
            __instance.UpdatePlayerPositionToBox2DPosition(ms);
        }

        // Update shake
        Vector2 vector = __instance.Position;
        __instance.Shake.UpdateShake(ms / __instance.GameWorld.SlowmotionHandler.SlowmotionModifier);
        vector = __instance.Shake.ApplyShake(vector);

        // Update hurt level if we
        // arent a burnt corpse
        int hurtLevel = 0;
        if (!__instance.Burned)
        {
            float fullness = __instance.Health.Fullness;
            hurtLevel = ((__instance.Health.Fullness <= 0.12f) ? 2 : ((fullness <= 0.25f) ? 1 : 0));
            // hurtLevel = ((__instance.Health.Fullness <= 0.12f) ? 2 : ((fullness <= 0.25f) ? 1 : 0));
        }
        __instance.Equipment.EnsureHurtLevelEquipped(hurtLevel);

        // Smoothly grow/shrink draw scale
        // if it changed.
        float drawScale = __instance.DrawScale;
        if (__instance.m_currentDrawScale != drawScale)
        {
            // Instantly set it if we are just created.
            if (__instance.GameWorld.ElapsedTotalRealTime - __instance.CreateTime < 100f)
            {
                __instance.m_currentDrawScale = __instance.DrawScale;
            }
            else if (__instance.m_currentDrawScale < drawScale)
            {
                __instance.m_currentDrawScale += 0.0003f * ms;
                if (__instance.m_currentDrawScale > drawScale)
                {
                    __instance.m_currentDrawScale = drawScale;
                }
            }
            else
            {
                __instance.m_currentDrawScale -= 0.0003f * ms;
                if (__instance.m_currentDrawScale < drawScale)
                {
                    __instance.m_currentDrawScale = drawScale;
                }
            }
        }
        
        Vector2 drawPosition = __instance.Shake.ApplyShake(__instance.Position);
        Color drawColor = GetPlayerDrawColor(__instance);

        // Draw Speedboost's delayed copy
        if (__instance.SpeedBoostActive)
        {
            // Speedboost copy is see-through
            Color SB_drawColor = drawColor;
            drawColor.A = 40;

            // Update delayed copy position
            Vector2 vec2 = (drawPosition - __instance.m_speedBoostDelayedPos);
            float num = vec2.CalcSafeLengthSquared(); // Minor optimization, use SqrLength instead of Length
            if (num > 6f)
            {
                vec2.Normalize();
                if (vec2.IsValid())
                {
                    __instance.m_speedBoostDelayedPos = drawPosition - vec2 * 5.99f;
                }
            }
            else
            {
                vec2.Normalize();
                if (vec2.IsValid())
                {
                    // Minor optimization, multiply by 1/13 instead of ms/13
                    __instance.m_speedBoostDelayedPos += vec2 * Math.Min(ms * 0.0769230769f, (float)Math.Sqrt(num));
                }
            }

            // Draw speedboost copy
            __instance.m_subAnimations[0].Draw(
                spriteBatch,
                __instance.m_speedBoostDelayedPos,
                __instance.m_currentDrawScale,
                __instance.GetAnimationDirection(),
                __instance.Rotation + __instance.m_subAnimations[0].Rotation,
                __instance.Equipment,
                SB_drawColor,
                ms
            );
        }

        // Draw player
        __instance.m_subAnimations[0].Draw(
            spriteBatch,
            drawPosition,
            __instance.m_currentDrawScale,
            __instance.GetAnimationDirection(),
            __instance.Rotation + __instance.m_subAnimations[0].Rotation,
            __instance.Equipment,
            drawColor,
            ms
        );

        return false;
    }
    */
}
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