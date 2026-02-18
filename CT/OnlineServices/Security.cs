using HarmonyLib;
using SDR.Networking;
using SFD;
using SFD.MenuControls;

namespace SFDCT.OnlineServices;

[HarmonyPatch]
internal static class Security
{
    internal static bool IsInvalidGameServer(GameServerInfo gameServer)
    {
        bool invalidMaxPlayers = gameServer.MaxAvailableSlots == 0
                                    || gameServer.MaxAvailableSlots > 16;

        int totalPlayerCount = gameServer.Players + gameServer.Bots;
        bool invalidPlayerCount = totalPlayerCount > gameServer.MaxAvailableSlots;

        bool invalidNameLength = gameServer.GameName == null
                                    || gameServer.GameName.Length < 3
                                    || gameServer.GameName.Length > 24;

        return invalidMaxPlayers || invalidPlayerCount || invalidNameLength;
    }

    // Large chat messages cause stuttering on other clients, reject those messages as spam
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Server), nameof(Server.HandleChatMessage))]
    private static void Server_HandleChatMessage_Postfix_SecurityChecks(ref bool __result, GameUser senderGameUser, string stringMsg)
    {
        if (!__result) return;

        var maxChars = GameChat.m_textbox?.maxChars ?? GameChat.m_textbox.maxChars;
        if (stringMsg.Length > maxChars) __result = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameBrowserPanel), nameof(GameBrowserPanel.IncludeGameInFilter))]
    private static void GameBrowserPanel_IncludeGameInFilter_Postfix_SecurityChecks(ref bool __result, GameServerInfo gameServer)
    {
        if (__result) __result = !IsInvalidGameServer(gameServer);
    }
}
