using Microsoft.Xna.Framework;
using SFD;
using SFD.MenuControls;
using HarmonyLib;

namespace SFDCT.OnlineServices;

[HarmonyPatch]
internal static class Browser
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameBrowserMenuItem), nameof(GameBrowserMenuItem.Game), MethodType.Setter)]
    private static bool PatchBrowser(SFDGameServerInstance value, GameBrowserMenuItem __instance)
    {
        if (__instance.m_game != value)
        {
            __instance.m_game = value;
            if (__instance.labels != null)
            {
                Color color = Constants.COLORS.RED;
                if (__instance.m_game != null && __instance.m_game.SFDGameServer != null && !Security.FilterSFDGameServer(__instance.m_game.SFDGameServer))
                {
                    if (__instance.m_game.SFDGameServer.Version == Constants.VERSION)
                    {
                        color = Color.White;
                    }

                    if (__instance.m_game.SFDGameServer.Version.StartsWith("v.2"))
                    {
                        color = new Color(222, 66, 165);
                    }

                    if (__instance.m_game.SFDGameServer.Players <= 0)
                    {
                        color *= 0.50f;
                    }
                    else if (__instance.m_game.SFDGameServer.Players == __instance.m_game.SFDGameServer.MaxPlayers)
                    {
                        color *= 0.70f;
                    }
                }
                else
                {
                    color *= 0.25f;
                }

                foreach (Label label in __instance.labels)
                {
                    label.Color = color;
                }
            }
        }

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameBrowserPanel), nameof(GameBrowserPanel.IncludeGameInFilter))]
    private static void FilterBrowser(ref bool __result, GameBrowserPanel __instance, SFDGameServerInstance gameServer)
    {
        if (__result)
        {
            __result = !Security.FilterSFDGameServer(gameServer.SFDGameServer);
        }
    }
}