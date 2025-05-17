using HarmonyLib;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using SFD;
using SFD.MLGameAuthorization;
using SFDCT.Game;
using SFDCT.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SFDCT.Sync;

[HarmonyPatch]
internal static class ServerHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Server), nameof(Server.updateForcedServerMovement))]
    private static bool ServerUpdateForcedServerMovement(Server __instance, float time)
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

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Server), nameof(Server.SendMessage), [typeof(MessageType), typeof(object), typeof(NetConnection), typeof(NetConnection)])]
    private static IEnumerable<CodeInstruction> ServerSendMessage(IEnumerable<CodeInstruction> instructions)
    {
        for (int i = 0; i < 8; i++)
        {
            instructions.ElementAt(100 + i).opcode = OpCodes.Nop;
        }

        return instructions;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Server), nameof(Server.HandleDataMessage))]
    private static IEnumerable<CodeInstruction> ServerHandleDataMessage(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new(instructions);

        for (int i = 0; i < 8; i++)
        {
            code[66 + i].opcode = OpCodes.Nop;
        }

        for (int i = 0; i < 9; i++)
        {
            code[425 + i].opcode = OpCodes.Nop;
        }

        code.Insert(105, new CodeInstruction(OpCodes.Ldarg_0, null));
        code.Insert(106, new CodeInstruction(OpCodes.Ldarg_2, null));
        code.Insert(107, new CodeInstruction(OpCodes.Ldloc_2, null));
        code.Insert(108, new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(ServerHandler), nameof(ServerHandleCustomSignal), [typeof(Server), typeof(NetIncomingMessage), typeof(NetMessage.Signal.Data)])));

        return code;
    }

    private static void ServerHandleCustomSignal(Server server, NetIncomingMessage msg, NetMessage.Signal.Data msgSignalData)
    {
        switch (msgSignalData.Signal)
        {
            case (NetMessage.Signal.Type)30:
                object[] data = (object[])msgSignalData.Object;
                CustomSignalType customSignalType = (CustomSignalType)((int)data[0]);

                // ConsoleOutput.ShowMessage(ConsoleOutputType.Information, "Server: Receiving custom signal " + customSignalType);
                switch (customSignalType)
                {
                    case CustomSignalType.DebugMouseUpdateSignal:
                        if (msg.GameConnectionTag() == null || !(msg.GameConnectionTag().IsModerator || msg.GameConnectionTag().IsHost)) break;

                        DebugMouseUpdateSignalData customSignalData = DebugMouseUpdateSignalData.Get(data);
                        WorldHandler.UpdateUserDebugMouse(customSignalData.ID, new Vector2(customSignalData.X, customSignalData.Y), customSignalData.Pressed, customSignalData.Delete);
                        break;
                }
                break;
        }
    }
}
