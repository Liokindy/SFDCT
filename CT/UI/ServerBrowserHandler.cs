using HarmonyLib;
using Microsoft.Xna.Framework;
using SFD.MenuControls;

namespace SFDCT.OnlineServices;

[HarmonyPatch]
internal static class ServerBrowserHandler
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameBrowserMenuItem), nameof(GameBrowserMenuItem.Game), MethodType.Setter)]
    private static void GameBrowserMenuItem_Setter_Game_Postfix_CustomServerColors(GameBrowserMenuItem __instance)
    {
        if (__instance.labels == null) return;
        if (__instance.m_game == null) return;
        if (__instance.m_game.SFDGameServer == null) return;

        var game = __instance.m_game.SFDGameServer;

        bool isInvalid = false;
        bool isSFR = false;
        bool isEmpty = false;
        bool isFull = false;

        if (Security.IsInvalidGameServer(game))
        {
            isInvalid = true;
        }
        else
        {
            if (game.Version.StartsWith("v.2"))
            {
                isSFR = true;
            }

            if (game.Players <= 0)
            {
                isEmpty = true;
            }
            else if (game.Players >= game.MaxPlayers)
            {
                isFull = true;
            }
        }

        foreach (var label in __instance.labels)
        {
            if (isSFR)
            {
                label.Color = new Color(222, 66, 165);
            }

            if (isInvalid)
            {
                label.Color *= 0.25f;
                continue;
            }

            if (isEmpty)
            {
                label.Color *= 0.5f;
                continue;
            }

            if (isFull)
            {
                label.Color *= 0.7f;
                continue;
            }
        }
    }
}