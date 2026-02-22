using HarmonyLib;
using Lidgren.Network;
using Microsoft.Xna.Framework;
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
            foreach (var connection in __instance.m_server.Connections)
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
                        Logger.LogDebug($"ProfileChange {gameUser.AccountName} '{gameUser.GetProfileName()}'");
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

    internal static bool HandleConnectRequest(Server server, NetConnection connection, ref NetMessage.Connection.ConnectRequest.Data connectData)
    {
        if (connectData.AsSpectators && connectData.ConnectingUserCount > 1)
        {
            connection.Disconnect(Constants.NET.SERVER_BYE_MESSAGE);
            return true;
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Server), nameof(Server.HandleDataMessage))]
    private static bool Server_HandleDataMessage_Prefix_CustomSignals(Server __instance, NetMessage.MessageData messageData, NetIncomingMessage msg)
    {
        if (messageData.MessageType == MessageHandler.SFDCTMessageType)
        {
            var data = MessageHandler.Read(msg, messageData);
            HandleCustomMessage(__instance, data, msg, msg.SenderConnection);
            return false;
        }

        return true;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Server), nameof(Server.DoReadRun))]
    private static IEnumerable<CodeInstruction> Server_DoReadRun_Transpiler_SpectatorFix(ILGenerator il, IEnumerable<CodeInstruction> instructions)
    {
        var code = new List<CodeInstruction>(instructions);

        // SFD accepts spectators that aren't the host, however 'JoinedAsSpectator'
        // is only set for the host if 'IsServer' is true, if 'IsGame' is true
        // then SFD hard codes 'JoinedAsSpectator' as false even though the
        // connection data may have 'AsSpectator' as true, and it will be accepted
        var gameUserLocalIndex = 37;
        var connectDataLocalIndex = 19;
        var senderConnectionLocalIndex = 18;

        var isGameInstruction = code[773];
        var isServerInstruction = code[779];
        var isLocalHostInstruction = code[455];
        var whileLoopEndInstruction = code[1686];
        var afterLobbyHelpTextInstruction = code[873];

        var asSpectatorInstructions = new List<CodeInstruction>
        {
            new(OpCodes.Ldloc_S, gameUserLocalIndex),
            new(OpCodes.Ldloc_S, connectDataLocalIndex), // connectData
            new(OpCodes.Ldfld, AccessTools.Field(typeof(NetMessage.Connection.ConnectRequest.Data), nameof(NetMessage.Connection.ConnectRequest.Data.AsSpectators))),
            new(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(GameUser), nameof(GameUser.JoinedAsSpectator)))
        };

        // Replace 'false'
        var isGameIndex = code.IndexOf(isGameInstruction);
        code.RemoveRange(isGameIndex, 3);
        code.InsertRange(isGameIndex, asSpectatorInstructions);

        // Replace 'flag2 &&'
        var isServerIndex = code.IndexOf(isServerInstruction);
        code.RemoveRange(isServerIndex, 8);
        code.InsertRange(isServerIndex, asSpectatorInstructions);

        // This is after reading connectData, doing some vanilla checks,
        // but before game slots are searched
        var continueWhileLoopLabel = il.DefineLabel();

        whileLoopEndInstruction.labels.Add(continueWhileLoopLabel);

        var handleConnectRequestInstructions = new List<CodeInstruction>
        {
            new(OpCodes.Ldarg_0), // this (Server)
            new(OpCodes.Ldloc_S, senderConnectionLocalIndex), // ref senderConnection
            new(OpCodes.Ldloca_S, connectDataLocalIndex), // ref data
            new(OpCodes.Call, AccessTools.Method(typeof(ServerHandler), nameof(HandleConnectRequest))),
            new(OpCodes.Brtrue, continueWhileLoopLabel),
        };

        var isLocalHostIndex = code.IndexOf(isLocalHostInstruction);
        code.InsertRange(isLocalHostIndex, handleConnectRequestInstructions);

        // Add help text
        // ChatMessage.Show(LanguageHelper.GetText("sfdct.menu.lobby.helpText"), Color.Yellow, "", false);
        var afterLobbyHelpTextIndex = code.IndexOf(afterLobbyHelpTextInstruction);
        var showCTHelpTextInstructions = new List<CodeInstruction>
        {
            new(OpCodes.Ldstr, "sfdct.menu.lobby.helpText"),
            new(OpCodes.Call, AccessTools.Method(typeof(LanguageHelper), nameof(LanguageHelper.GetText), [typeof(string)])),
            new(OpCodes.Call, AccessTools.PropertyGetter(typeof(Color), nameof(Color.Yellow))),
            new(OpCodes.Ldstr, ""),
            new(OpCodes.Ldc_I4_0),
            new(OpCodes.Call, AccessTools.Method(typeof(ChatMessage), nameof(ChatMessage.Show), [typeof(string), typeof(Color), typeof(string), typeof(bool)]))
        };

        code.InsertRange(afterLobbyHelpTextIndex, showCTHelpTextInstructions);
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
