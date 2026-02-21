using HarmonyLib;
using Microsoft.Xna.Framework;
using Networking.LidgrenAdapter;
using SDR.Networking;
using SFD;
using SFDCT.Game;
using SFDCT.Helper;
using SFDCT.Sync.Data;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SFDCT.Sync;

[HarmonyPatch]
internal static class ServerHandler
{
    internal const float SERVER_MOVEMENT_TOGGLE_TIME_MS_FORCE_TRUE = -1f;
    internal const float SERVER_MOVEMENT_TOGGLE_TIME_MS_FORCE_FALSE = -2f;

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
        var updateTime = 100f;

        __instance.m_updateForcedServerMovementTime -= time;
        if (__instance.m_updateForcedServerMovementTime > 0) return false;
        __instance.m_updateForcedServerMovementTime = updateTime;

        __instance.m_forcedServerMovementConnectionCount = 0;
        var serverMovementPing = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING * 0.001f;

        var serverUpdate = Server.ServerUpdateLockObject;
        lock (serverUpdate)
        {
            foreach (var connection in __instance.m_server.GetNetConnections(true))
            {
                var tag = connection.GameConnectionTag();
                if (tag == null) continue;

                var fromForce = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING == 0;
                var fromPing = connection.AverageRoundtripTime > serverMovementPing || Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING == 0;

                var useServerMovement = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_CHECK && (fromPing || fromForce);
                useServerMovement = (tag.ForcedServerMovementToggleTime == SERVER_MOVEMENT_TOGGLE_TIME_MS_FORCE_TRUE) || (tag.ForcedServerMovementToggleTime != SERVER_MOVEMENT_TOGGLE_TIME_MS_FORCE_FALSE && useServerMovement);

                if (tag.ForceServerMovement == useServerMovement)
                {
                    tag.ForcedServerMovementToggleTime = 0f;
                    continue;
                }

                tag.ForcedServerMovementToggleTime += updateTime;
                if (fromForce || tag.ForcedServerMovementToggleTime > Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_TOGGLE_TIME_MS)
                {
                    tag.ForceServerMovement = useServerMovement;

                    if (tag.GameUsers != null)
                    {
                        foreach (var user in tag.GameUsers)
                        {
                            user.ForceServerMovement = useServerMovement;

                            var player = __instance.GameInfo?.GameWorld?.GetPlayerByUserIdentifier(user.UserIdentifier);
                            player?.UpdateCanDoPlayerAction();
                        }
                    }
                }

                if (tag.ForceServerMovement) __instance.m_forcedServerMovementConnectionCount++;
            }
        }

        return false;
    }

    internal static void HandleCustomMessage(Server server, SFDCTMessageData messageData, NetIncomingMessage incomingMessage, NetConnection connection)
    {
        GameConnectionTag incomingTag = connection.GameConnectionTag();

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
                        Logger.LogDebug($"ProfileChange '{gameUser.GetProfileName()}'");
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

    internal static bool HandleServerResponse(ref ServerResponse serverResponse, Server server, NetMessage.Connection.DiscoveryConnectRequest.Data connectData)
    {
        if (connectData.AsSpectators && connectData.ConnectingUserCount > 1)
        {
            serverResponse = ServerResponse.RefuseConnect;
            return true;
        }

        return false;
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

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Server), nameof(Server.ConnectionRequestAttachToGameSlots))]
    private static IEnumerable<CodeInstruction> Server_ConnectionRequestAttachToGameSlots_Transpiler_SpectatorFix(IEnumerable<CodeInstruction> instructions)
    {
        // SFD accepts spectators that aren't the host, however 'JoinedAsSpectator'
        // is only set for the host if 'IsServer' is true, if 'IsGame' is true
        // then SFD hard codes 'JoinedAsSpectator' as false even though the
        // connection data may have 'AsSpectator' as true, and it will be accepted

        var code = new List<CodeInstruction>(instructions);
        var gameUserLocalIndex = 10;

        var isGameInstruction = code[181];
        var isServerInstruction = code[187];

        var asSpectatorInstructions = new List<CodeInstruction>
        {
            new(OpCodes.Ldloc_S, gameUserLocalIndex),
            new(OpCodes.Ldarg_2), // connectData
            new(OpCodes.Ldfld, AccessTools.Field(typeof(NetMessage.Connection.DiscoveryConnectRequest.Data), nameof(NetMessage.Connection.DiscoveryConnectRequest.Data.AsSpectators))),
            new(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(GameUser), nameof(GameUser.JoinedAsSpectator)))
        };

        // Replace 'false'
        var isGameIndex = code.IndexOf(isGameInstruction);
        code.RemoveRange(isGameIndex, 3);
        code.InsertRange(isGameIndex, asSpectatorInstructions);

        // Replace 'netConnection.IsHost &&'
        var isServerIndex = code.IndexOf(isServerInstruction);
        code.RemoveRange(isServerIndex, 9);
        code.InsertRange(isServerIndex, asSpectatorInstructions);

        return code;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Server), nameof(Server.HandleMessageDiscoveryConnectRequest))]
    private static IEnumerable<CodeInstruction> Server_HandleMessageDiscoveryConnectRequest_Transpiler_InjectHandleResponse(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        // This is after reading connectData, doing some vanilla checks,
        // but before game slots are searched
        var code = new List<CodeInstruction>(instructions);

        var finalSendDiscoveryConnectResponseLabel = il.DefineLabel();
        var connectDataLocalIndex = 4;
        var serverResponseLocalIndex = 2;

        code[285].labels.Add(finalSendDiscoveryConnectResponseLabel);

        var targetIndex = 149;
        var targetInstructions = new List<CodeInstruction>
        {
            new(OpCodes.Ldloca_S, serverResponseLocalIndex), // ref serverResponse
            new(OpCodes.Ldarg_0), // this (Server)
            new(OpCodes.Ldloc_S, connectDataLocalIndex), // data
            new(OpCodes.Call, AccessTools.Method(typeof(ServerHandler), nameof(HandleServerResponse))),
            new(OpCodes.Brtrue, finalSendDiscoveryConnectResponseLabel),
        };

        code.InsertRange(targetIndex, targetInstructions);
        return code;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Server), nameof(Server.Shutdown), [typeof(bool)])]
    private static void Server_Shutdown_Prefix(Server __instance)
    {
        DebugMouse = false;
        DebugMouseList.Clear();
    }
}
