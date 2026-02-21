using HarmonyLib;
using Lidgren.Network;
using SFD;
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
    [HarmonyPatch(typeof(Client), nameof(Client.CreateConnectRequestMessage))]
    private static bool Client_CreateConnectRequestMessage_Prefix_RequestAsSpectator(ref NetOutgoingMessage __result, Client __instance, string cryptPhraseB)
    {
        if (NextConnectionAsSpectator)
        {
            NextConnectionAsSpectator = false;

            var outgoingMessage = __instance.m_client.CreateMessage();
            var asSpectator = true;
            var playerCount = GameInfo.LocalPlayerCount;
            var activePlayerIndex = GameInfo.GetActiveLocalUserIndexes();
            var activePlayerProfiles = GameInfo.GetActiveLocalUserProfiles();
            var accountData = Constants.Account.CreateAccountData(cryptPhraseB + Constants.Account.AccountSignature);

            var data = new NetMessage.Connection.ConnectRequest.Data(Constants.PApplicationInstance, Constants.SApplicationInstance, playerCount, activePlayerIndex, activePlayerProfiles, asSpectator, accountData, Constants.CLIENT_REQUEST_SERVER_MOVEMENT);
            __result = NetMessage.Connection.ConnectRequest.Write(ref data, outgoingMessage);
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
