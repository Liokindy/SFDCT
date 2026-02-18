using HarmonyLib;
using Microsoft.Xna.Framework;
using Networking.LidgrenAdapter;
using SDR.Networking;
using SFD;
using SFDCT.Game;
using SFDCT.Helper;
using SFDCT.Sync.Data;
using SteamLayer.SteamManagers;
using Steamworks;
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
            if (field != value && GameSFD.Handle.Server != null && GameSFD.Handle.Server.Running)
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
        if (__instance.m_updateForcedServerMovementTime > 0) return false;

        __instance.m_updateForcedServerMovementTime = 100f;

        int serverMovementCount = 0;
        float serverMovementThreshold = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING * 0.001f;

        object serverUpdateLockObject = Server.ServerUpdateLockObject;
        lock (serverUpdateLockObject)
        {
            List<NetConnection> netConnections = __instance.m_server.GetNetConnections(true);
            for (int i = netConnections.Count - 1; i >= 0; i--)
            {
                NetConnection netConnection = netConnections[i];
                GameConnectionTag connectionTag = netConnection.GameConnectionTag();
                if (connectionTag == null) continue;

                bool useServerMovement = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_CHECK && (netConnection.AverageRoundtripTime > serverMovementThreshold - (connectionTag.ForceServerMovement ? 0.01f : 0f) | Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING == 0);

                if (connectionTag.ForcedServerMovementToggleTime == -1f) useServerMovement = true;
                if (connectionTag.ForcedServerMovementToggleTime == -2f) useServerMovement = false;

                if (connectionTag.ForceServerMovement == useServerMovement)
                {
                    connectionTag.ForcedServerMovementToggleTime = 0f;
                }
                else
                {
                    connectionTag.ForcedServerMovementToggleTime += 100f;
                    if (connectionTag.ForcedServerMovementToggleTime > Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_TOGGLE_TIME_MS || Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING == 0)
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

        return false;
    }

    internal static void HandleCustomMessage(Server server, SFDCTMessageData messageData, NetIncomingMessage incomingMessage, NetConnection connection)
    {
        GameConnectionTag incomingTag = connection.ConnectionTag;

        switch (messageData.Type)
        {
            case MessageHandler.SFDCTMessageDataType.DebugMouseUpdate:
                if (incomingTag == null) break;

                if (incomingTag.IsHost || (incomingTag.IsModerator && !DebugMouseOnlyHost))
                {
                    var box2DPosition = new Vector2((float)messageData.Data[0], (float)messageData.Data[1]);
                    var pressed = (bool)messageData.Data[2];

                    DebugMouse tagDebugMouse = null;
                    foreach (var debugMouse in DebugMouseList)
                    {
                        if (debugMouse.Tag == incomingTag)
                        {
                            tagDebugMouse = debugMouse;
                            break;
                        }
                    }

                    if (tagDebugMouse == null)
                    {
                        tagDebugMouse = new();
                        tagDebugMouse.Tag = incomingTag;

                        DebugMouseList.Add(tagDebugMouse);
                    }

                    tagDebugMouse.LastNetUpdateTime = (float)NetTime.Now;
                    tagDebugMouse.Box2DPosition = box2DPosition;
                    tagDebugMouse.Pressed = pressed;
                }
                break;
            case MessageHandler.SFDCTMessageDataType.ProfileChangeRequest:
                if (incomingTag == null) break;

                var userIndex = (int)messageData.Data[0];

                if (userIndex >= 0 && userIndex < incomingTag.GameUsers.Length)
                {
                    var gameUser = incomingTag.GameUsers[userIndex];
                    var profile = (Profile)messageData.Data[1];

                    if (gameUser != null && profile != null)
                    {
#if DEBUG
                        string accountName = string.Empty;
                        if (server.GameInfo.AccountNameInfo.TryGetAccountID(gameUser.UserIdentifier, out SteamId steamId))
                        {
                            accountName = SteamIdNameManager.Instance.GetAccountName(steamId);
                        }

                        Logger.LogDebug($"ProfileChange {accountName} '{gameUser.GetProfileName()}'");
#endif

                        gameUser.Profile = profile;
                        gameUser.Profile.Updated = true;

                        server.SyncGameUserInfo(gameUser);

                        if (gameUser.GameSlot != null)
                        {
                            server.SyncGameSlotInfo(gameUser.GameSlot);
                        }
                        MessageHandler.Send(server, new(MessageHandler.SFDCTMessageDataType.ProfileChangeRequest));
                    }
                }
                break;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Server), nameof(Server.HandleDataMessage))]
    private static bool Server_HandleDataMessage_Prefix_CustomSignals(Server __instance, NetMessage.MessageData messageData, NetIncomingMessage msg, NetConnection netConnection)
    {
        if (messageData.MessageType == MessageHandler.SFDCTMessageType)
        {
            var data = MessageHandler.Read(msg, messageData);
            HandleCustomMessage(__instance, data, msg, netConnection);
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
