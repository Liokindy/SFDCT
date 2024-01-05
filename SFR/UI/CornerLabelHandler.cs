using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.MenuControls;
using CSecurity = SFR.Misc.Constants.Security;

namespace SFR.UI;

/// <summary>
///     Patches the labels shown on the corner of the screen.
///     i.e: version, account, etc
/// </summary>
[HarmonyPatch]
internal static class CornerLabelHandler
{
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.DrawInner))]
    private static IEnumerable<CodeInstruction> VersionLabel(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(76).operand = Misc.Constants.SFRVersion;
        return code;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MainMenuPanel), nameof(MainMenuPanel.DrawLoggedInAs))]
    public static bool OnlineAsLabel(SpriteBatch batch)
    {
        // Draw "Online as"
        if (SFD.SteamIntegration.Steam.GetIsOnline())
        {
            if (MainMenuPanel.m_textLoggedInAs == null)
            {
                MainMenuPanel.m_textLoggedInAs = LanguageHelper.GetText("menu.mainMenu.onlineAs").Trim();
                MainMenuPanel.m_textLoggedInAsSize = Constants.MeasureString(Constants.Font1, MainMenuPanel.m_textLoggedInAs + " ");
            }
            Vector2 labelPos = new Vector2(4f, GameSFD.GAME_HEIGHT - 40f);
            Constants.DrawString(batch, Constants.Font1, MainMenuPanel.m_textLoggedInAs, labelPos, Color.Gray);

            if (CSecurity.CanUseObfuscatedNames)
            {
                Constants.DrawString(batch, Constants.Font1, Constants.Account.NameRaw, labelPos + new Vector2(MainMenuPanel.m_textLoggedInAsSize.X, 0f), Constants.COLORS.YELLOW);
                float obfuscatedNameWidth = Constants.MeasureString(Constants.Font1, Constants.Account.NameRaw + " ").X;
                Constants.DrawString(batch, Constants.Font1, "("+CSecurity.RealPersonaName+")", labelPos + new Vector2(MainMenuPanel.m_textLoggedInAsSize.X + obfuscatedNameWidth, 0f), new Color(128,128,128));
            }
            else
            {

                Constants.DrawString(batch, Constants.Font1, Constants.Account.NameRaw, labelPos + new Vector2(MainMenuPanel.m_textLoggedInAsSize.X, 0f), Constants.COLORS.YELLOW);
            }

            return false;
        }

        // Offline
        if (MainMenuPanel.m_textOffline == null)
        {
            MainMenuPanel.m_textOffline = LanguageHelper.GetText("menu.mainMenu.offline").Trim();
        }
        Vector2 position = new Vector2(4f, GameSFD.GAME_HEIGHT - 40f);
        Constants.DrawString(batch, Constants.Font1, MainMenuPanel.m_textOffline, position, new Color(128, 128, 128));
        
        return false;
    }
}
