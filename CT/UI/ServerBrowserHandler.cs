using HarmonyLib;
using Microsoft.Xna.Framework;
using SFD;
using SFD.MenuControls;

namespace SFDCT.OnlineServices;

[HarmonyPatch]
internal static class ServerBrowserHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameBrowserMenuItem), nameof(GameBrowserMenuItem.Game), MethodType.Setter)]
    private static bool GameBrowserMenuItem_Setter_Game_Prefix_CustomServerColors(SFDGameServerInstance value, GameBrowserMenuItem __instance)
    {
        if (__instance.m_game != value)
        {
            __instance.m_game = value;

            if (__instance.labels != null && __instance.m_game != null && __instance.m_game.SFDGameServer != null)
            {
                Color color = Constants.COLORS.RED;

                if (Security.FilterSFDGameServer(__instance.m_game.SFDGameServer))
                {
                    color *= 0.25f;
                }
                else
                {
                    if (__instance.m_game.SFDGameServer.Version == Constants.VERSION)
                    {
                        color = Color.White;
                    }

                    // Check if it's an SFR server
                    if (__instance.m_game.SFDGameServer.Version.StartsWith("v.2"))
                    {
                        color = new Color(222, 66, 165);
                    }

                    // Empty servers are darkened and full servers are slightly darkened
                    if (__instance.m_game.SFDGameServer.Players <= 0)
                    {
                        color *= 0.50f;
                    }
                    else if (__instance.m_game.SFDGameServer.Players >= __instance.m_game.SFDGameServer.MaxPlayers)
                    {
                        color *= 0.70f;
                    }
                }

                foreach (Label label in __instance.labels)
                {
                    label.Color = color;
                }
            }
        }

        return false;
    }
}