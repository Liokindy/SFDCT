using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Lidgren.Network;
using SFD;
using SFDCT.Helper;
using CConst = SFDCT.Misc.Constants;
using CSecurity = SFDCT.Misc.Constants.Security;
using CSettings = SFDCT.Settings.Values;

namespace SFDCT.Sync;

[HarmonyPatch]
internal static class Host
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.GET_HOST_GAME_SLOT_STATE))]
    private static bool Slots_GetGameSlotState(ref byte __result, int index)
    {
        __result = CConst.HOST_GAME_SLOT_STATES[index];
        return false;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.GET_HOST_GAME_SLOT_TEAM))]
    private static bool Slots_GetGameSlotTeam(ref Team __result, int index)
    {
        __result = CConst.HOST_GAME_SLOT_TEAMS[index];
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.SET_HOST_GAME_SLOT_STATE))]
    private static bool Slots_GetGameSlotState(int index, byte value)
    {
        CConst.HOST_GAME_SLOT_STATES[index] = value;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Constants), nameof(Constants.SET_HOST_GAME_SLOT_TEAM))]
    private static bool Slots_GetGameSlotTeam(int index, Team value)
    {
        CConst.HOST_GAME_SLOT_TEAMS[index] = value;
        return false;
    }


    // Increase GameInfo.GameSlots array size
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameInfo), MethodType.Constructor, new Type[] { typeof(GameOwnerEnum) })]
    private static IEnumerable<CodeInstruction> Slots_GameSlotsArray(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(94).opcode = OpCodes.Ldc_I4_S;
        code.ElementAt(94).operand = CConst.HOST_GAME_SLOT_COUNT;
        return code;
    }

    // Increase maximum connections
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Server), nameof(Server.Start))]
    private static IEnumerable<CodeInstruction> Server_Start(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(26).opcode = OpCodes.Ldc_I4_S;
        code.ElementAt(26).operand = CConst.HOST_GAME_SLOT_COUNT + 1;
        return code;
    }

    // Init all slots
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.InitOpenGameSlots))]
    private static IEnumerable<CodeInstruction> Slots_InitOpenSlots(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> code = new List<CodeInstruction>(instructions);
        code.ElementAt(54).opcode = OpCodes.Ldc_I4_S;
        code.ElementAt(54).operand = CConst.HOST_GAME_SLOT_COUNT;
        return code;
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

    // Steam Persona
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SFD.SteamIntegration.Steam), nameof(SFD.SteamIntegration.Steam.PersonaNameShort), MethodType.Getter)]
    private static void Get_PersonaNameShort(ref string __result)
    {
        if (!CSecurity.ValidateObfuscatedName(CSettings.GetString("OBFUSCATED_HOST_ACCOUNT_NAME"), out string errorMessage))
        {
            CSettings.SetSetting("OBFUSCATED_HOST_ACCOUNT_NAME", "Unnamed");
            CSettings.SetSetting("USE_OBFUSCATED_HOST_ACCOUNT_NAME", false);
            return;
        }

        if (CSecurity.CanUseObfuscatedNames && !(GameSFD.Handle.m_waterlogo != "" || SFD.Constants.Account.ID == 666U || CSecurity.RealPersonaName.Contains("666:")))
        {
            if (CSecurity.RealPersonaName != __result.ToString())
            {
                CSecurity.RealPersonaName = __result.ToString();
            }
            __result = CSettings.GetString("OBFUSCATED_HOST_ACCOUNT_NAME");
        }
    }
}