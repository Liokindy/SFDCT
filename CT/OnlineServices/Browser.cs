using Microsoft.Xna.Framework;
using SFD.MenuControls;
using SFDCT.Misc;
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
            if (__instance.labels != null && __instance.m_game != null && __instance.m_game.SFDGameServer != null)
            {
                Color color = Color.White;
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

                foreach (Label label in __instance.labels)
                {
                    label.Color = color;
                }
            }
        }

        return false;
    }
}