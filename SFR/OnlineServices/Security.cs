using System;
using HarmonyLib;
using SFD;
using SFDCT.Helper;

namespace SFDCT.OnlineServices;

[HarmonyPatch]
internal static class Security
{
    //     Profiles names from users are only validated for integrity,
    //     meaning they can be empty. This validates the name for this,
    //     reserved names, etc.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Profile), nameof(Profile.ValidateProfileIntegrity))]
    private static void ValidateProfileIntegrity(Profile __instance, bool validateNameIntegrity)
    {
        if (!validateNameIntegrity) { return; }

        if (!Profile.ValidateName(__instance.Name, out string result, out string errorMsg))
        {
            Logger.LogDebug($"Profile.ValidateName: name: '{__instance.Name}' result: '{result}' errorMsg: '{errorMsg}'");
            __instance.Name = result;
        }
    }

    //     Modified clients can enter the server and use an empty AccountName.
    //     This can make them harder to track and kick/ban, the only use for
    //     this is malicious so we deny their account negotation.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Constants.Account), nameof(Constants.Account.ReadAccountData))]
    private static void ReadAccountData(ref bool __result, byte[] accountData, string key, ref string accountName, ref string account)
    {
        string mess = $"Constants.Account.ReadAccountData: key: '{key}' accountName: '{accountName}' account: '{account}'";

        if (!__result)
        {
            Logger.LogDebug(mess);
            return;
        }

        bool b666usersMustContain666 = false;
        if (string.IsNullOrEmpty(accountName) || string.IsNullOrWhiteSpace(accountName) || accountName == "  " || accountName.Length <= 2 || accountName.Length >= 24 || (b666usersMustContain666 && account == "S666" && !accountName.StartsWith("666:")))
        {
            __result = false;
            Logger.LogDebug(mess);
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
            Logger.LogDebug(mess);
            return;
        }
    }

    //     Modified clients can bypass the chat box's 120 character limit.
    //     Large chat messages cause stuttering on other clients, so we
    //     reject those messages as spam.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Server), nameof(Server.HandleChatMessage))]
    private static void HandleChatMessage(ref bool __result, GameUser senderGameUser, string stringMsg)
    {
        if (__result && stringMsg.Length > 120)
        {
            __result = false;
        }
    }
}
