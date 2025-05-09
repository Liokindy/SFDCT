using System;
using HarmonyLib;
using SFD;
using SFDCT.Helper;

namespace SFDCT.OnlineServices;

[HarmonyPatch]
internal static class Security
{
    internal static bool doLimitAccountNameLength = true;
    internal static bool doTryEnforce666On666Users = true;
    internal static bool doCheck666On666Users = true;
    internal static bool doCheckEmptyAccountName = true;
    internal static bool doExtraProfileNameValidation = true;
    internal static bool doLimitChatMessageLength = true;

    //     Profiles names from users are only validated for integrity,
    //     meaning they can be empty. This validates the name for this,
    //     reserved names, etc.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Profile), nameof(Profile.ValidateProfileIntegrity))]
    private static void ValidateProfileIntegrity(Profile __instance, bool validateNameIntegrity)
    {
        if (!validateNameIntegrity || !doExtraProfileNameValidation)
        {
            return;
        }

        if (!Profile.ValidateName(__instance.Name, out string result, out string errorMsg))
        {
            __instance.Name = result;

            Logger.LogDebug($"[SECURITY] False ValidateProfileIntegrity: '{__instance.Name}', '{result}', '{errorMsg}'");
            return;
        }
    }

    //     Modified clients can enter the server and use an empty AccountName.
    //     This can make them harder to track and kick/ban, the only use for
    //     this is malicious so we deny their account negotation.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Constants.Account), nameof(Constants.Account.ReadAccountData))]
    private static void ReadAccountData(ref bool __result, byte[] accountData, string key, ref string accountName, ref string account)
    {
        byte failedAtStep = 0;
        string mess = "[SECURITY] False ReadAccountData at {0}: '{1}', {2}, '{3}', '{4}'";

        if (!__result)
        {
            Logger.LogDebug(string.Format(mess, failedAtStep, key, accountData.Length, accountName, account));
            return;
        }

        bool doLimitAccountNameLength = true;
        bool doTryEnforce666On666Users = true;
        bool doCheck666On666Users = true;
        bool doCheckEmptyAccountName = true;

        if (doTryEnforce666On666Users)
        {
            if ((account == "S666" && !accountName.StartsWith("666:")) || (account != "S666" && accountName.StartsWith("666:")))
            {
                if (account != "S666" && accountName.StartsWith("666:"))
                {
                    account = "S666";
                }
                else
                {
                    accountName = "666:" + accountName;
                }
            }
        }

        if (doCheck666On666Users)
        {
            if ((account == "S666" && !accountName.StartsWith("666:")) || (account != "S666" && accountName.StartsWith("666:")))
            {
                __result = false;
                Logger.LogDebug(string.Format(mess, failedAtStep, key, accountData.Length, accountName, account));
                return;
            }
        }

        if (doCheckEmptyAccountName)
        {
            if (string.IsNullOrEmpty(accountName) || string.IsNullOrWhiteSpace(accountName) || accountName == "  ")
            {
                __result = false;
                Logger.LogDebug(string.Format(mess, failedAtStep, key, accountData.Length, accountName, account));
                return;
            }

            string accName = accountName;
            accName = accName.Replace(Environment.NewLine, "");
            accName = accName.Replace("\r", "");
            accName = accName.Replace("\n", "");
            accName = accName.Trim();
            while (accName.Contains("  "))
            {
                accName = accName.Replace("  ", " ");
            }

            if (string.IsNullOrEmpty(accountName) || string.IsNullOrWhiteSpace(accName))
            {
                __result = false;
                Logger.LogDebug(string.Format(mess, failedAtStep, key, accountData.Length, accountName, account));
                return;
            }
        }

        if (doLimitAccountNameLength)
        {
            if (accountName.Length <= 2 || accountName.Length >= 24)
            {
                __result = false;
                Logger.LogDebug(string.Format(mess, failedAtStep, key, accountData.Length, accountName, account));
                return;
            }
        }
    }

    //     Modified clients can bypass the chat box's 120 character limit.
    //     Large chat messages cause stuttering on other clients, so we
    //     reject those messages as spam.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Server), nameof(Server.HandleChatMessage))]
    private static void HandleChatMessage(ref bool __result, GameUser senderGameUser, string stringMsg)
    {
        if (!__result || !doLimitChatMessageLength)
        {
            return;
        }

        if (stringMsg.Length > 120)
        {
            __result = false;

            string mess = $"[SECURITY] Failed HandleChatMessage: '{senderGameUser.AccountName}':'{senderGameUser.Account}' {senderGameUser.GetNetIP()}, {stringMsg.Length}";
            Logger.LogDebug(mess);
        }
    }
}
