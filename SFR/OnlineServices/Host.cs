using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using CSecurity = SFR.Misc.Constants.Security;
using SFRSettings = SFR.Settings.Values;

namespace SFR.Sync;

[HarmonyPatch]
internal static class Host
{
    /*
    [HarmonyTranspiler]
    [HarmonyPatch( typeof(SFD.Server), nameof(SFD.Server.DoReadRun) )]
    private static IEnumerable<CodeInstruction> DoReadRun(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(375).opcode = OpCodes.Ldloc_S;
        code.ElementAt(375).operand = 25;
        code.ElementAt(376).opcode = OpCodes.Ldstr;
        code.ElementAt(376).operand = "kkk";
        return code;
    }
    */
    /*
    [HarmonyTranspiler]
    [HarmonyPatch( typeof(NetMessage.GameInfo.GameUserUpdate), nameof(NetMessage.GameInfo.GameUserUpdate.Write) )]
    private static IEnumerable<CodeInstruction> Write(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.RemoveRange(67, 2);

        code.ElementAt(66).opcode = OpCodes.Ldstr;
        code.ElementAt(66).operand = "kkk";
        return code;
    }
    */

    // Not used in-game.
    /*
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SFD.SteamIntegration.Steam), nameof(SFD.SteamIntegration.Steam.PersonaName), MethodType.Getter)]
    private static void Get_PersonaName(ref string __result)
    {
        if (CSecurity.UseObsucatedNames)
        {
            __result = CSecurity.ObfuscatedPersonaName;
        }
    }
    */

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SFD.SteamIntegration.Steam), nameof(SFD.SteamIntegration.Steam.PersonaNameShort), MethodType.Getter)]
    private static void Get_PersonaNameShort(ref string __result)
    {
        if (CSecurity.CanUseObfuscatedNames && !(GameSFD.Handle.m_waterlogo != "" || SFD.Constants.Account.ID == 666U || CSecurity.RealPersonaName.Contains("666:")))
        {
            if (CSecurity.RealPersonaName != __result.ToString())
            {
                CSecurity.RealPersonaName = __result.ToString();
            }
            __result = SFRSettings.GetString("OBFUSCATED_HOST_ACCOUNT_NAME");
        }
    }
}