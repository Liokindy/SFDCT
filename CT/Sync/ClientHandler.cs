using HarmonyLib;
using Lidgren.Network;
using SFD;
using SFDCT.Sync.Data;

namespace SFDCT.Sync;

[HarmonyPatch]
internal static class ClientHandler
{
    internal static bool DebugMouse;

    internal static void HandleCustomMessage(Client client, SFDCTMessageData messageData, NetIncomingMessage incomingMessage)
    {
        switch (messageData.Type)
        {
            case MessageHandler.SFDCTMessageDataType.DebugMouseToggle:
                bool enabled = (bool)messageData.Data[0];

                DebugMouse = enabled;
                break;
        }
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
