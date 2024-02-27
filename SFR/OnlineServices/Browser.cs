using System;
using System.Collections.Generic;
using HarmonyLib;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using SFD;
using SFD.MenuControls;
using SFD.SFDOnlineServices;
using Constants = SFDCT.Misc.Constants;

namespace SFDCT.OnlineServices;

/// <summary>
///     Handles in-game browser and server join requests.
/// </summary>
[HarmonyPatch]
internal static class Browser
{
    public static Color ServerNormal = Color.White;
    public static Color ServerError = Color.Red;
    public static Color ServerSFR = new(222, 66, 165);

    public static float ServerFullMultiplier = 0.70f;
    public static float ServerEmptyMultiplier = 0.50f;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameBrowserMenuItem), nameof(GameBrowserMenuItem.Game), MethodType.Setter)]
    private static bool PatchBrowser(SFDGameServerInstance value, GameBrowserMenuItem __instance)
    {
        if (__instance.m_game != value)
        {
            __instance.m_game = value;
            if (__instance.labels != null && __instance.m_game != null && __instance.m_game.SFDGameServer != null)
            {
                var color = ServerError;
                if (__instance.m_game.SFDGameServer.Version == Constants.Version.SFD)
                {
                    color = ServerNormal;
                }
                else if (__instance.m_game.SFDGameServer.Version.StartsWith("v.2"))
                {
                    color = ServerSFR;
                }
                
                if (__instance.m_game.SFDGameServer.Players <= 0)
                {
                    color *= ServerEmptyMultiplier;
                }
                else if (__instance.m_game.SFDGameServer.Players == __instance.m_game.SFDGameServer.MaxPlayers)
                {
                    color *= ServerFullMultiplier;
                }

                // Third party programs can send false servers with false information
                // to the server browser, causing a lot of clutter.
                if (__instance.m_game.SFDGameServer.Players > 32 || __instance.m_game.SFDGameServer.MaxPlayers > 32 ||
                    (__instance.m_game.SFDGameServer.Players > __instance.m_game.SFDGameServer.MaxPlayers)
                )
                {
                    color = ServerError;
                    color *= ServerEmptyMultiplier;
                }

                foreach (Label label in __instance.labels)
                {
                    label.Color = color;
                }
            }
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFDGameServer), nameof(SFDGameServer.Version), MethodType.Setter)]
    private static bool GameServerVersion(ref string value)
    {
        if (value == "v.1.3.7x")
        {
            value = Constants.Version.SFD;
        }

        return true;
    }

   
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SFD.Constants), nameof(SFD.Constants.VersionCheckDifference), typeof(string))]
    private static bool VersionCheckPatch(string versionToCheck, ref VersionDifference __result)
    {
        __result = versionToCheck == Constants.Version.SFD ? VersionDifference.Same : VersionDifference.Older;

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NetMessage.Connection.DiscoveryResponse.Data), MethodType.Constructor, typeof(ServerResponse), typeof(string), typeof(string), typeof(string), typeof(Guid))]
    private static void PatchServerVersionResponse(ref NetMessage.Connection.DiscoveryResponse.Data __instance)
    {
        if (__instance.Version == "v.1.3.7x")
        {
            __instance.Version = Constants.Version.SFD;
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
                instruction.operand = Constants.Version.SFD;
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
            __instance.Version = Constants.Version.SFD;
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

            result.ServerPInstance = result.Version == Constants.Version.SFD ? new Guid(netIncomingMessage.ReadBytes(16)) : Guid.Empty;
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