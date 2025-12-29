using HarmonyLib;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using SFD;
using SFDCT.Game;
using SFDCT.Sync.Data;
using System.Collections.Generic;

namespace SFDCT.Sync;

[HarmonyPatch]
internal static class ServerHandler
{
    internal static List<DebugMouse> DebugMouseList = [];
    internal static bool DebugMouseOnlyHost = false;
    internal static bool DebugMouse
    {
        get;
        set
        {
            if (field != value && GameSFD.Handle.Server != null)
            {
                var data = new SFDCTMessageData();
                data.Type = MessageHandler.SFDCTMessageDataType.DebugMouseToggle;
                data.Data = [value];

                MessageHandler.Send(GameSFD.Handle.Server, data);
            }

            field = value;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Server), nameof(Server.updateForcedServerMovement))]
    private static bool Server_updateForcedServerMovement_Prefix_CustomServerMovement(Server __instance, float time)
    {
        __instance.m_updateForcedServerMovementTime -= time;
        if (__instance.m_updateForcedServerMovementTime <= 0f)
        {
            __instance.m_updateForcedServerMovementTime = 100f;

            int serverMovementCount = 0;
            float serverMovementThreshold = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING * 0.001f;

            object serverUpdateLockObject = Server.ServerUpdateLockObject;
            lock (serverUpdateLockObject)
            {
                List<NetConnection> connections = __instance.m_server.Connections;
                foreach (var connection in connections)
                {
                    GameConnectionTag connectionTag = connection.GameConnectionTag();
                    if (connectionTag == null) continue;

                    bool useServerMovement = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_CHECK && (connectionTag.Ping > serverMovementThreshold - (connectionTag.ForceServerMovement ? 0.01f : 0f) | Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING == 0);

                    if (connectionTag.ForcedServerMovementToggleTime == -1f) useServerMovement = true;
                    if (connectionTag.ForcedServerMovementToggleTime == -2f) useServerMovement = false;

                    if (connectionTag.ForceServerMovement == useServerMovement)
                    {
                        connectionTag.ForcedServerMovementToggleTime = 0f;
                    }
                    else
                    {
                        if (connectionTag.ForcedServerMovementToggleTime > 0) connectionTag.ForcedServerMovementToggleTime += 100f;

                        if (connectionTag.ForcedServerMovementToggleTime < 0f && connectionTag.ForcedServerMovementToggleTime > Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_TOGGLE_TIME_MS || Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING == 0)
                        {
                            connectionTag.ForceServerMovement = useServerMovement;
                            if (connectionTag.GameUsers != null)
                            {
                                foreach (var connectionUser in connectionTag.GameUsers)
                                {
                                    connectionUser.ForceServerMovement = useServerMovement;

                                    Player playerByUserIdentifier = __instance.GameInfo.GameWorld.GetPlayerByUserIdentifier(connectionUser.UserIdentifier);
                                    playerByUserIdentifier?.UpdateCanDoPlayerAction();
                                }
                            }
                        }

                        if (connectionTag.ForceServerMovement) serverMovementCount++;
                    }
                }
            }
        }

        return false;
    }

    internal static void HandleCustomMessage(Server server, SFDCTMessageData messageData, NetIncomingMessage incomingMessage)
    {
        switch (messageData.Type)
        {
            default:
                break;
            case MessageHandler.SFDCTMessageDataType.DebugMouseUpdate:
                GameConnectionTag incomingTag = incomingMessage.GameConnectionTag();
                if (incomingTag == null) break;

                if (incomingTag.IsHost || (incomingTag.IsModerator && !DebugMouseOnlyHost))
                {
                    Vector2 box2DPosition = new((float)messageData.Data[0], (float)messageData.Data[1]);
                    bool pressed = (bool)messageData.Data[2];

                    DebugMouse tagDebugMouse = null;
                    foreach (var debugMouse in DebugMouseList)
                    {
                        if (debugMouse.Tag == incomingMessage.GameConnectionTag())
                        {
                            tagDebugMouse = debugMouse;
                            break;
                        }
                    }

                    if (tagDebugMouse == null)
                    {
                        tagDebugMouse = new();
                        tagDebugMouse.Tag = incomingMessage.GameConnectionTag();

                        DebugMouseList.Add(tagDebugMouse);
                    }

                    tagDebugMouse.LastNetUpdateTime = (float)NetTime.Now;
                    tagDebugMouse.Box2DPosition = box2DPosition;
                    tagDebugMouse.Pressed = pressed;
                }
                break;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Server), nameof(Server.HandleDataMessage))]
    private static bool Server_HandleDataMessage_Prefix_CustomSignals(Server __instance, NetMessage.MessageData messageData, NetIncomingMessage msg)
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
    [HarmonyPatch(typeof(Server), nameof(Server.Shutdown), [typeof(bool)])]
    private static void Server_Shutdown_Prefix(Server __instance)
    {
        DebugMouse = false;
        DebugMouseList.Clear();
    }
}
