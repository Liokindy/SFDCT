using HarmonyLib;
using SFD;
using SFD.SFDOnlineServices;
using SFDCT.Configuration;
using System;

namespace SFDCT.OnlineServices;

[HarmonyPatch]
internal static class Security
{
    internal static bool IsInvalidGameServer(SFDGameServer gameServer)
    {
        if (gameServer == null) return true;

        bool invalidMaxPlayers = gameServer.MaxPlayers == 0
                                    || gameServer.MaxPlayers > 16;

        int totalPlayerCount = gameServer.Players + gameServer.Bots;
        bool invalidPlayerCount = totalPlayerCount > gameServer.MaxPlayers;

        bool invalidNameLength = gameServer.GameName == null
                                    || gameServer.GameName.Length < 3
                                    || gameServer.GameName.Length > 24;

        return invalidMaxPlayers || invalidPlayerCount || invalidNameLength;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Constants.Account), nameof(Constants.Account.ReadAccountData))]
    private static void Constants_Account_Postfix_ReadAccountData(ref bool __result, byte[] accountData, string key, ref string accountName, ref string account)
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.ExtraAccountDataChecking)) return;

        // Already failed by vanilla checks
        if (!__result) return;

        // Modified clients can enter the server and use an empty AccountName,
        // this can make them harder to track and kick/ban, *try* to deny
        // their account negotation
        byte failedAtStep = 0;
        string failedMessage = "SFDCT: AccountData deny at {0}, key '{1}', data '{2}', name '{3}', account '{4}'";

        bool doLimitAccountNameLength = true;
        bool doEnforce666On666Users = true;
        bool doCheck666On666Users = true;
        bool doCheckEmptyAccountName = true;

        if (doEnforce666On666Users)
        {
            if (!account.StartsWith("S666") && accountName.StartsWith("666:"))
            {
                account = "S666";
            }
            else if (account.StartsWith("S666") && !accountName.StartsWith("666:"))
            {
                accountName = "666:" + accountName;
            }
        }

        if (doCheck666On666Users)
        {
            if ((account == "S666" && !accountName.StartsWith("666:")) || (account != "S666" && accountName.StartsWith("666:")))
            {
                __result = false;
                ConsoleOutput.ShowMessage(ConsoleOutputType.Information, string.Format(failedMessage, failedAtStep, key, accountData.Length, accountName, account));
                return;
            }
        }

        if (doCheckEmptyAccountName)
        {
            if (string.IsNullOrEmpty(accountName) || string.IsNullOrWhiteSpace(accountName) || accountName == "  ")
            {
                __result = false;
                ConsoleOutput.ShowMessage(ConsoleOutputType.Information, string.Format(failedMessage, failedAtStep, key, accountData.Length, accountName, account));
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
                ConsoleOutput.ShowMessage(ConsoleOutputType.Information, string.Format(failedMessage, failedAtStep, key, accountData.Length, accountName, account));
                return;
            }
        }

        if (doLimitAccountNameLength)
        {
            if (accountName.Length <= 2 || accountName.Length >= 24)
            {
                __result = false;
                ConsoleOutput.ShowMessage(ConsoleOutputType.Information, string.Format(failedMessage, failedAtStep, key, accountData.Length, accountName, account));
                return;
            }
        }
    }
}
