using HarmonyLib;
using SFD;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class UserHandler
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameUser), nameof(GameUser.IsDedicatedPreview), MethodType.Getter)]
    private static void GameUser_Getter_IsDedicatedPreview_Postfix_ChangeDSCheck(GameUser __instance, ref bool __result)
    {
        // The server creates a "local" and "remote" at
        // 'Server.SetupServerUsers()', these game users
        // are used by the dedicated server preview, they
        // count as the host, join as a spectator, have
        // an empty account name and their user identifiers
        // are set to 1

        __result = __result && __instance.UserIdentifier == 1;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameUser), nameof(GameUser.CanWin), MethodType.Getter)]
    private static void GameUser_Getter_CanWin_Postfix_SpectatorFix(GameUser __instance, ref bool __result)
    {
        // Add a check for regular spectators, they shouldn't be able to win
        // because they wont be spawned next match

        __result = __result && !__instance.JoinedAsSpectator;
    }

    internal static bool IsNotJoinedAsSpectatorAndIsSpectatingWhileWaitingToPlay(GameUser user)
    {
        return !user.JoinedAsSpectator && user.SpectatingWhileWaitingToPlay;
    }
}
