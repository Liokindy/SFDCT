using HarmonyLib;
using Networking.LidgrenAdapter;
using SFD;
using SFD.SteamIntegration;
using SFDCT.Sync.Data;

namespace SFDCT.Sync;

[HarmonyPatch]
internal static class ClientHandler
{
    internal static bool DebugMouse;
    internal static bool NextConnectionAsSpectator;

    internal static void HandleCustomMessage(Client client, SFDCTMessageData messageData, NetIncomingMessage incomingMessage)
    {
        switch (messageData.Type)
        {
            case MessageHandler.SFDCTMessageDataType.DebugMouseToggle:
                DebugMouse = (bool)messageData.Data[0];
                break;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Client), nameof(Client.CreateDiscoveryConnectRequestMessage))]
    private static bool Client_CreateDiscoveryConnectRequestMessage_Prefix_RequestAsSpectator(ref NetOutgoingMessage __result, Client __instance, NetOutgoingMessage nom, string passphrase)
    {
        if (NextConnectionAsSpectator)
        {
            NextConnectionAsSpectator = false;

            var asSpectator = true;
            var playerCount = GameInfo.LocalPlayerCount;
            var activePlayerIndex = GameInfo.GetActiveLocalUserIndexes();
            var activePlayerProfiles = GameInfo.GetActiveLocalUserProfiles();
            var personaShortName = SteamInfo.GetPersonaNameShort();

            __result = NetMessage.Connection.DiscoveryConnectRequest.Write(new(Constants.PApplicationInstance, Constants.SApplicationInstance, 9, "v.1.5.0", passphrase, playerCount, activePlayerIndex, activePlayerProfiles, asSpectator, personaShortName, Constants.CLIENT_REQUEST_SERVER_MOVEMENT), nom);
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Client), nameof(Client.HandleDataMessage))]
    private static bool Client_HandleDataMessage_Prefix_CustomMessages(Client __instance, NetMessage.MessageData messageData, NetIncomingMessage msg, bool processGameWorldDependentData = true)
    {
        if (messageData.MessageType == MessageHandler.SFDCTMessageType)
        {
            var data = MessageHandler.Read(msg, messageData);
            HandleCustomMessage(__instance, data, msg);
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Client), nameof(Client.Shutdown))]
    private static void Client_Shutdown_Prefix(Client __instance)
    {
        DebugMouse = false;
    }
}
