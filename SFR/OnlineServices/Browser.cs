using System;
using System.Collections.Generic;
using HarmonyLib;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using SFD;
using SFD.MenuControls;
using SFD.SFDOnlineServices;
using CConst = SFDCT.Misc.Globals;

namespace SFDCT.OnlineServices;

/// <summary>
///     Handles in-game browser and server join requests.
/// </summary>
[HarmonyPatch]
internal static class Browser
{
    private static Color ServerSFR = new(222, 66, 165);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameBrowserMenuItem), nameof(GameBrowserMenuItem.Game), MethodType.Setter)]
    private static bool PatchBrowser(SFDGameServerInstance value, GameBrowserMenuItem __instance)
    {
        if (__instance.m_game != value)
        {
            __instance.m_game = value;
            if (__instance.labels != null && __instance.m_game != null && __instance.m_game.SFDGameServer != null)
            {
                var color = Constants.COLORS.RED;
                if (__instance.m_game.SFDGameServer.Version == CConst.Version.SFD)
                {
                    color = Color.White;
                }
                else if (__instance.m_game.SFDGameServer.Version.StartsWith("v.2"))
                {
                    color = ServerSFR;
                }
                
                // While using extended-slots, the client expects network data in
                // a way the server will might not send it, causing problems.
                if (CConst.HOST_GAME_EXTENDED_SLOTS && __instance.m_game.SFDGameServer.MaxPlayers != CConst.SLOTCOUNT)
                {
                    color = Constants.COLORS.RED;
                }

                if (__instance.m_game.SFDGameServer.Players <= 0)
                {
                    color *= 0.50f;
                }
                else if (__instance.m_game.SFDGameServer.Players == __instance.m_game.SFDGameServer.MaxPlayers)
                {
                    color *= 0.70f;
                }

                foreach (Label label in __instance.labels)
                {
                    label.Color = color;
                }
            }
        }

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameBrowserPanel), nameof(GameBrowserPanel.refreshTimer_Elapsed))]
    private static void PatchBrowser(GameBrowserPanel __instance)
    {
        try
        {
            // Third party programs can send false servers with false information
            // to the server browser, causing a lot of clutter, hide them

            List<SFDGameServerInstance> gameServerInstances = new List<SFDGameServerInstance>();
            for (int i = 0; i < __instance.m_browserGameServers.Count; i++)
            {
                SFDGameServerInstance serverInstance = __instance.m_browserGameServers[i];

                bool exclude = false;
                if (serverInstance != null && serverInstance.SFDGameServer != null)
                {
                    if (serverInstance.SFDGameServer.Players > 32 || serverInstance.SFDGameServer.MaxPlayers > 32 || serverInstance.SFDGameServer.Players > serverInstance.SFDGameServer.MaxPlayers)
                    {
                        exclude = true;
                    }
                    if (serverInstance.SFDGameServer.Description.Trim().Replace(" ", "").Length > 200 || serverInstance.SFDGameServer.GameName.Trim().Replace(" ", "").Length <= 3)
                    {
                        exclude = true;
                    }
                }

                if (!exclude)
                {
                    gameServerInstances.Add(serverInstance);
                }
            }

            __instance.m_browserGameServers.Clear();
            __instance.m_browserGameServers = gameServerInstances;
        }
        catch
        {
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(JoinGamePanel), nameof(JoinGamePanel.connect))]
    private static bool JoinGamePanelConnectToGame(JoinGamePanel __instance)
    {
        if (CConst.HOST_GAME_EXTENDED_SLOTS && __instance.m_server != null)
        {
            if (__instance.m_server.MaxPlayers != CConst.SLOTCOUNT)
            {
                MessageStack.Show("Your slot count is different from the server.", MessageStackType.Error);
                return false;
            }
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFDGameServer), nameof(SFDGameServer.Version), MethodType.Setter)]
    private static bool GameServerVersion(ref string value)
    {
        if (value == "v.1.3.7x")
        {
            value = CConst.Version.SFD;
        }

        return true;
    }

   
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFD.Constants), nameof(SFD.Constants.VersionCheckDifference), typeof(string))]
    private static bool VersionCheckPatch(string versionToCheck, ref VersionDifference __result)
    {
        __result = versionToCheck == CConst.Version.SFD ? VersionDifference.Same : VersionDifference.Older;

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NetMessage.Connection.DiscoveryResponse.Data), MethodType.Constructor, typeof(ServerResponse), typeof(string), typeof(string), typeof(string), typeof(Guid))]
    private static void PatchServerVersionResponse(ref NetMessage.Connection.DiscoveryResponse.Data __instance)
    {
        if (__instance.Version == "v.1.3.7x")
        {
            __instance.Version = CConst.Version.SFD;
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Server), nameof(Server.DoReadRun))]
    private static IEnumerable<CodeInstruction> ServerReadRun(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.operand == null)
            {
                continue;
            }

            if (instruction.operand.Equals("v.1.3.7x"))
            {
                instruction.operand = CConst.Version.SFD;
            }
        }

        return instructions;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NetMessage.Connection.DiscoveryRequest.Data), MethodType.Constructor, typeof(Guid), typeof(int), typeof(bool), typeof(string), typeof(string))]
    private static void PatchServerVersionRequest(ref NetMessage.Connection.DiscoveryRequest.Data __instance)
    {
        if (__instance.Version == "v.1.3.7x")
        {
            __instance.Version = CConst.Version.SFD;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NetMessage.Connection.DiscoveryResponse), nameof(NetMessage.Connection.DiscoveryResponse.Read))]
    private static bool PatchServerVersion(NetIncomingMessage netIncomingMessage, ref NetMessage.Connection.DiscoveryResponse.Data __result)
    {
        NetMessage.Connection.DiscoveryResponse.Data result = default;
        try
        {
            result.Version = netIncomingMessage.ReadString();
            result.Response = (ServerResponse)netIncomingMessage.ReadInt32();

            if (netIncomingMessage.Position < netIncomingMessage.LengthBits)
            {
                result.CryptPhraseA = netIncomingMessage.ReadString();
            }

            if (netIncomingMessage.Position < netIncomingMessage.LengthBits)
            {
                result.CryptPhraseB = netIncomingMessage.ReadString();
            }

            result.ServerPInstance = result.Version == CConst.Version.SFD ? new Guid(netIncomingMessage.ReadBytes(16)) : Guid.Empty;
        }
        catch (Exception)
        {
            result.Version = "Unknown";
            result.Response = ServerResponse.RefuseConnectVersionDiffer;
            result.ServerPInstance = Guid.Empty;
        }

        __result = result;

        return false;
    }
}