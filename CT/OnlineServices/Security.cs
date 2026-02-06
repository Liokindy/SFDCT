using HarmonyLib;
using SFD;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SFDCT.OnlineServices;

[HarmonyPatch]
internal static class Security
{
    internal static bool IsInvalidGameServer(SFD.SFDOnlineServices.SFDGameServer gameServer)
    {
        if (gameServer == null) return true;

        bool invalidMaxPlayers = gameServer.MaxPlayers == 0
                                    || gameServer.MaxPlayers > 16;

        bool invalidPlayerCount = gameServer.Players > gameServer.MaxPlayers
                                    || gameServer.Bots > gameServer.MaxPlayers
                                    || (gameServer.Bots + gameServer.Players) > gameServer.MaxPlayers;

        bool invalidNameLength = gameServer.GameName == null
                                    || gameServer.GameName.Length < 3
                                    || gameServer.GameName.Length > 24;

        return invalidMaxPlayers || invalidPlayerCount || invalidNameLength;
    }

    // This allows the DS server to bypass the ReadAccountData check in
    // order to join the server while having an empty AccountName.
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Server), nameof(Server.DoReadRun))]
    private static IEnumerable<CodeInstruction> Server_DoReadRun_Transpiler_SecurityChecks(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        // We add a Brtrue_S (brach if true) before the Account Validation check
        // so the DSPreview Client can join the server even though it does not
        // send a valid account to the server

        // We do this by using "flag2" that stores if the sender connection
        // is localhost. It is stored as local var 22

        Label returnLabel = il.DefineLabel();

        var code = new List<CodeInstruction>(instructions);
        code[614].labels.Add(returnLabel);

        code.Insert(612, new(OpCodes.Ldloc_S, 22));
        code.Insert(613, new(OpCodes.Brtrue_S, returnLabel));
        return code;
    }

    // Profiles names from users are only validated for integrity,
    // meaning they can be empty. This validates the name for this,
    // reserved names, etc.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Profile), nameof(Profile.ValidateProfileIntegrity))]
    private static void Profile_ValidateProfileIntegrity_Prefix_SecurityChecks(Profile __instance, bool validateNameIntegrity)
    {
        if (!validateNameIntegrity) return;
        if (!Profile.ValidateName(__instance.Name, out string result, out string errorMsg)) __instance.Name = result;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Constants.Account), nameof(Constants.Account.ReadAccountData))]
    private static void Constants_Account_ReadAccountData_Postfix_SecurityChecks(ref bool __result, byte[] accountData, string key, ref string accountName, ref string account)
    {
        if (!__result) return;

        // Try to stop invisible/empty accounts
        var accName = accountName.Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "").Trim();
        while (accName.Contains("  "))
        {
            accName = accName.Replace("  ", " ");
        }

        if (string.IsNullOrEmpty(accountName) || string.IsNullOrWhiteSpace(accountName) || string.IsNullOrEmpty(account) || string.IsNullOrWhiteSpace(account))
        {
            __result = false;
            return;
        }

        // If the ID or Name is "cracky", but not the other, then try to enforce them both
        if (account != "S666" && accountName.StartsWith("666:"))
        {
            account = "S666";
        }
        else if (account == "S666" && !accountName.StartsWith("666:"))
        {
            accountName = "666:" + accountName;
        }

        // These are Steam's name limits
        if (accountName.Length <= 2 || accountName.Length >= 32)
        {
            __result = false;
            return;
        }
    }

    // Large chat messages cause stuttering on other clients, reject those messages as spam
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Server), nameof(Server.HandleChatMessage))]
    private static void Server_HandleChatMessage_Postfix_SecurityChecks(ref bool __result, GameUser senderGameUser, string stringMsg)
    {
        if (!__result) return;

        var maxChars = GameChat.m_textbox?.maxChars ?? 120;
        if (stringMsg.Length > maxChars) __result = false;
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(SFD.MenuControls.GameBrowserPanel), nameof(SFD.MenuControls.GameBrowserPanel.IncludeGameInFilter))]
    private static void GameBrowserPanel_IncludeGameInFilter_Postfix_SecurityChecks(ref bool __result, SFD.MenuControls.GameBrowserPanel __instance, SFD.MenuControls.SFDGameServerInstance gameServer)
    {
        if (__result) __result = !IsInvalidGameServer(gameServer.SFDGameServer);
    }
}
