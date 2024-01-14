using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using SFD;
using CSecurity = SFDCT.Misc.Constants.Security;
using SFRSettings = SFDCT.Settings.Values;

namespace SFDCT.Sync;

[HarmonyPatch]
internal static class Host
{
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