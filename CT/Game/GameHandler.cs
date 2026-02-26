using HarmonyLib;
using SFD;
using System.Linq;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class GameHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.TotalGameUserCount), MethodType.Getter)]
    private static bool GameInfo_Getter_TotalGameUserCount_Prefix_SpectatorFix(GameInfo __instance, ref int __result)
    {
        // Original method only checks if the user isn't considered
        // the dedicated server preview

        __result = __instance.GetGameUsers().Count(u => u.IsUser && !u.JoinedAsSpectator);
        return false;
    }
}
