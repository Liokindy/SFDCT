using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using HarmonyLib;
using Lidgren.Network;
using SFD;
using SFDCT.Helper;
using CConst = SFDCT.Misc.Constants;
using CSettings = SFDCT.Settings.Values;

namespace SFDCT.Sync;

[HarmonyPatch]
internal static class Host
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.GET_HOST_GAME_SLOT_STATE))]
    private static bool Slots_GetGameSlotState(ref byte __result, int index)
    {
        if (CConst.HOST_GAME_SLOT_COUNT == 8) { return true; }

        __result = CConst.HOST_GAME_SLOT_STATES[index];
        return false;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.GET_HOST_GAME_SLOT_TEAM))]
    private static bool Slots_GetGameSlotTeam(ref Team __result, int index)
    {
        if (CConst.HOST_GAME_SLOT_COUNT == 8) { return true; }

        __result = CConst.HOST_GAME_SLOT_TEAMS[index];
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.SET_HOST_GAME_SLOT_STATE))]
    private static bool Slots_GetGameSlotState(int index, byte value)
    {
        if (CConst.HOST_GAME_SLOT_COUNT == 8) { return true; }

        CConst.HOST_GAME_SLOT_STATES[index] = value;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.SET_HOST_GAME_SLOT_TEAM))]
    private static bool Slots_GetGameSlotTeam(int index, Team value)
    {
        if (CConst.HOST_GAME_SLOT_COUNT == 8) { return true; }

        CConst.HOST_GAME_SLOT_TEAMS[index] = value;
        return false;
    }


    // Increase GameInfo.GameSlots array size
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameInfo), MethodType.Constructor, new Type[] { typeof(GameOwnerEnum) })]
    private static IEnumerable<CodeInstruction> Slots_GameSlotsArray(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(94).opcode = OpCodes.Ldc_I4_S;
        instructions.ElementAt(94).operand = CConst.HOST_GAME_SLOT_COUNT;
        return instructions;
    }

    // Increase maximum connections
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Server), nameof(Server.Start))]
    private static IEnumerable<CodeInstruction> Server_Start(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(26).opcode = OpCodes.Ldc_I4_S;
        instructions.ElementAt(26).operand = CConst.HOST_GAME_SLOT_COUNT + 4; // 12
        return instructions;
    }

    // Init all slots
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.InitOpenGameSlots))]
    private static IEnumerable<CodeInstruction> Slots_InitOpenSlots(IEnumerable<CodeInstruction> instructions)
    {
        instructions.ElementAt(54).opcode = OpCodes.Ldc_I4_S;
        instructions.ElementAt(54).operand = CConst.HOST_GAME_SLOT_COUNT;
        return instructions;
    }

    /// <summary>
    ///     Allow the host to send messages quickly
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameConnectionTag), nameof(GameConnectionTag.ConsumeFreeChatTicket))]
    private static bool GameConnectionTag_ConsumeFreeChatTicket(GameConnectionTag __instance, ref bool __result)
    {
        if (__instance.IsHost)
        {
            __result = true;
            return false;
        }
        return true;
    }

    /// <summary>
    ///     Users that have ForcedServerMovementToggleTime set to -1
    ///     will be have forced server movement regardless of latency.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Server), nameof(Server.updateForcedServerMovement))]
    private static bool Server_updateForcedServerMovement(Server __instance, float time)
    {
        __instance.m_updateForcedServerMovementTime -= time;
        if (__instance.m_updateForcedServerMovementTime > 0)
        {
            return false;
        }
        __instance.m_updateForcedServerMovementTime = 100f;

        int svMovCount = 0;
        bool svMovActive = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_CHECK;
        bool svMovForced = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING == 0;
        float svMovPing = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_PING * 0.001f;
        float svMovToggleTime = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_TOGGLE_TIME_MS;

        lock (Server.ServerUpdateLockObject)
        {
            List<NetConnection> netConns = __instance.m_server.Connections;
            for(int i = netConns.Count - 1; i >= 0; i--)
            {
                NetConnection netConn = netConns[i];
                GameConnectionTag gameConnTag = netConn.GameConnectionTag();
                if (gameConnTag == null)
                {
                    continue;
                }

                bool SvMovWasSet = (gameConnTag.ForcedServerMovementToggleTime == -1f);
                bool doSvMov = svMovForced || (svMovActive && gameConnTag.Ping > svMovPing);
                if (gameConnTag.ForceServerMovement != doSvMov && !SvMovWasSet)
                {
                    gameConnTag.ForcedServerMovementToggleTime += 100f;
                    if (svMovForced || gameConnTag.ForcedServerMovementToggleTime > svMovToggleTime)
                    {
                        gameConnTag.ForceServerMovement = doSvMov;
                        if (gameConnTag.GameUsers != null)
                        {
                            for(int j = 0; j < gameConnTag.GameUsers.Length; j++)
                            {
                                GameUser gu = gameConnTag.GameUsers[j];
                                gu.ForceServerMovement = doSvMov;

                                Player playerByUserID = __instance.GameInfo.GameWorld.GetPlayerByUserIdentifier(gu.UserIdentifier);
                                playerByUserID?.UpdateCanDoPlayerAction();
                            }
                        }
                    }
                }
                else
                {
                    if (!SvMovWasSet)
                    {
                        gameConnTag.ForcedServerMovementToggleTime = 0f;
                    }
                }

                if (gameConnTag.ForceServerMovement)
                {
                    svMovCount++;
                }
            }
        }
        __instance.m_forcedServerMovementConnectionCount = svMovCount;
        
        return false;
    }

    /// <summary>
    ///     Modified clients can enter the server and use an empty AccountName
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Constants.Account), nameof(Constants.Account.ReadAccountData))]
    private static void ReadAccountData(ref bool __result, byte[] accountData, string key, ref string accountName, ref string account)
    {
        if (string.IsNullOrEmpty(accountName) || string.IsNullOrWhiteSpace(accountName) || accountName.Length == 0)
        {
            // Possible modified client
            __result = false;
        }
    }

    /// <summary>
    ///     Modified clients can bypass the chat box's 120 character limit.
    ///     Large chat messages cause stuttering on other clients, so we
    ///     reject those messages as spam.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Server), nameof(Server.HandleChatMessage))]
    private static void Server_HandleChatMessage(ref bool __result, GameUser senderGameUser, string stringMsg)
    {
        // Message got denied by other means
        if (!__result)
        {
            return;
        }

        if (stringMsg.Length > 120)
        {
            __result = false;
        }
    }
}