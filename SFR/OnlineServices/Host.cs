using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Lidgren.Network;
using SFD;
using CConst = SFDCT.Misc.Globals;

namespace SFDCT.OnlineServices;

[HarmonyPatch]
internal static class Host
{
    //     Make GameUsers that have 'ForcedServerMovementToggleTime' set to -1
    //     have forced server movement regardless of their latency, makes it
    //     possible to set their ForcedServerMovement elsewhere
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Server), nameof(Server.updateForcedServerMovement))]
    private static bool ServerUpdateForcedServerMovement(Server __instance, float time)
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

    //     Make the dedicated server preview bypass the ReadAccountData check to
    //     join the server while having an invalid AccountName
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Server), nameof(Server.DoReadRun))]
    private static IEnumerable<CodeInstruction> ServerDoReadRunDSPreviewFix(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        // The "flag" variable stores if the sender is the DS preview.
        //
        // We add CodeInstructions that check for this variable (ldloc_S 20) and branches
        // after "flag3" is supposed to be set to false.
        //
        // We do this because "flag3" is only set to false after "ReadAccountData" returns false.
        // (Which it will, because the AccountName provided is empty.)

        // In-game code
        /*
        bool flag = false;
        .
        .
        else if (Program.IsServer)
        {
            flag = senderConnection.IsLocalHost();
        }
        .
        .
        if (!Constants.Account.ReadAccountData(connectData.AccountData, key, out accountName, out text))
        {
            flag3 = false;
        }
        */

        // Define a label in the instructions after "flag = false;"
        Label returnLabel = il.DefineLabel();
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);

        code.ElementAt(552).labels.Add(returnLabel);

        // Add the code instructions to branch if "flag" is true
        code.Insert(550, new(OpCodes.Ldloc_S, 20));
        code.Insert(551, new(OpCodes.Brtrue_S, returnLabel));

        return code;
    }
}