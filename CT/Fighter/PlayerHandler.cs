using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using HarmonyLib;
using System.Linq;
using SFDCT.Configuration;

namespace SFDCT.Fighter;

[HarmonyPatch]
internal static class PlayerHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerHUD), nameof(PlayerHUD.DrawName))]
    private static bool PlayerHUD_DrawName(PlayerHUD __instance, Player player, GameUser user, int x, int y, SpriteBatch spriteBatch, float elapsed)
    {
        if (__instance.m_nameLabelTeam != user.GameSlotTeam)
        {
            __instance.m_nameLabelTeam = user.GameSlotTeam;
        }

        // This is probably not the best way of doing this
        Color teamColor = Constants.GetTeamColor(__instance.m_nameLabelTeam);
        if (__instance.m_nameLabelTeam == Team.Independent && user != null)
        {
            if (CreditHandler.IsCredit(user.Account))
            {
                teamColor = CreditHandler.GetCreditColor(user.Account);
            }
        }
        __instance.m_nameLabel.Color = teamColor;

        __instance.m_nameLabel.DrawStatic(spriteBatch, new Vector2(x, y - 4));

        return false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Player), nameof(Player.Draw))]
    private static IEnumerable<CodeInstruction> Player_Draw(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(44).operand = Settings.Get<float>(SettingKey.LowHealthHurtLevel1Threshold);
        instructions.ElementAt(41).operand = Settings.Get<float>(SettingKey.LowHealthHurtLevel2Threshold);

        return instructions;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Player), nameof(Player.DrawPlates))]
    private static bool Player_DrawPlates(float ms, Player __instance)
    {
        Vector2 vector = Camera.ConvertWorldToScreen(__instance.Position + new Vector2(0f, 24f));
        float num = MathHelper.Max(Camera.Zoom * 0.5f, 1f);

        if (!__instance.IsDead)
        {
            if ((__instance.DrawStatusInfo & Player.DrawStatusInfoFlags.Name) == Player.DrawStatusInfoFlags.Name)
            {
                Color teamTextColor = __instance.GetTeamTextColor();

                if (__instance.CurrentTeam == Team.Independent)
                {
                    GameUser gameUser = __instance.GetGameUser();
                    if (gameUser != null && CreditHandler.IsCredit(gameUser.Account))
                    {
                        teamTextColor = CreditHandler.GetCreditColor(gameUser.Account);
                        teamTextColor = ColorCorrection.FromXNAToCustom(teamTextColor);
                    }
                }

                Constants.DrawString(__instance.m_spriteBatch, Constants.Font1Outline, __instance.Name, new Vector2(vector.X, vector.Y - 0.75f * __instance.m_nameTextSize.Y * num), teamTextColor, 0f, __instance.m_nameTextSize * 0.5f, num * 0.5f, SpriteEffects.None, 0);
                Texture2D teamIcon = Constants.GetTeamIcon(__instance.m_currentTeam);

                if (teamIcon != null)
                {
                    __instance.m_spriteBatch.Draw(teamIcon, new Vector2(vector.X - __instance.m_nameTextSize.X * 0.25f * num - (float)teamIcon.Width * num, vector.Y - __instance.m_nameTextSize.Y * num), null, Color.Gray, 0f, Vector2.Zero, num, SpriteEffects.None, 1f);
                }
            }
            if (__instance.ChatActive)
            {
                if (__instance.m_chatIconTimer > 250f)
                {
                    __instance.m_chatIconFrame = (__instance.m_chatIconFrame + 1) % 4;
                    __instance.m_chatIconTimer -= 250f;
                }
                else
                {
                    __instance.m_chatIconTimer += ms;
                }

                __instance.m_spriteBatch.Draw(Constants.ChatIcon, new Vector2(vector.X + __instance.m_nameTextSize.X * 0.25f * num, vector.Y - __instance.m_nameTextSize.Y * num), new Rectangle?(new Rectangle(1 + __instance.m_chatIconFrame * 13, 1, 12, 12)), ColorCorrection.FromXNAToCustom(Constants.COLORS.CHAT_ICON), 0f, Vector2.Zero, num, SpriteEffects.None, 1f);
            }
        }

        vector.Y -= 11f * num;
        if ((__instance.DrawStatusInfo & Player.DrawStatusInfoFlags.StatusBars) == Player.DrawStatusInfoFlags.StatusBars)
        {
            Player.HealthMode currentHealthMode = __instance.GetCurrentHealthMode();
            if (!__instance.IsDead)
            {
                BarMeter barMeter = __instance.Health;
                bool flag = barMeter.CheckRecentlyModified(2000f);
                Color xnaColor = Constants.COLORS.LIFE_BAR;
                if (currentHealthMode == Player.HealthMode.StrengthBoostOverHealth || currentHealthMode == Player.HealthMode.RocketRideOverHealth)
                {
                    barMeter = __instance.OverHealth;
                    xnaColor = (((int)(GameSFD.LastUpdateNetTimeMS / 200f) % 2 == 0) ? Constants.COLORS.LIFE_BAR_OVERHEALTH_A : Constants.COLORS.LIFE_BAR_OVERHEALTH_B);
                    flag = true;
                }
                if (flag | __instance.Energy.CheckRecentlyModified(2000f))
                {
                    float num2 = 32f * num;
                    float num3 = 2f * num;
                    Rectangle rectangle = new Rectangle((int)(vector.X - num2 / 2f), (int)vector.Y, (int)num2, (int)num3);
                    float num4 = Math.Max(1f, Camera.Zoom * 0.5f);
                    for (float num5 = -num4; num5 <= num4; num5 += num4 * 2f)
                    {
                        for (float num6 = -num4; num6 <= num4; num6 += num4 * 2f)
                        {
                            Rectangle destinationRectangle = new Rectangle(rectangle.X + (int)num5, rectangle.Y + (int)num6, rectangle.Width, (int)((float)rectangle.Height * 2f));
                            __instance.m_spriteBatch.Draw(Constants.WhitePixel, destinationRectangle, Color.Black);
                        }
                    }
                    __instance.m_spriteBatch.Draw(Constants.WhitePixel, rectangle, new Color(64, 64, 64));
                    if (barMeter.CheckRecentlyModified(50f))
                    {
                        xnaColor = Color.White;
                    }
                    int width = rectangle.Width;
                    rectangle.Width = (int)((float)width * barMeter.Fullness);
                    __instance.m_spriteBatch.Draw(Constants.WhitePixel, rectangle, ColorCorrection.FromXNAToCustom(xnaColor));
                    rectangle.Y += rectangle.Height;
                    rectangle.Width = width;
                    __instance.m_spriteBatch.Draw(Constants.WhitePixel, rectangle, new Color(64, 64, 64));
                    rectangle.Width = (int)((float)width * __instance.Energy.Fullness);
                    __instance.m_spriteBatch.Draw(Constants.WhitePixel, rectangle, ColorCorrection.FromXNAToCustom(Constants.COLORS.ENERGY_BAR));
                }
            }
        }

        return false;
    }
}
