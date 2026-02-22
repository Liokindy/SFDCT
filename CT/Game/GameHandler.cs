using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.Effects;
using SFD.GameKeyboard;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class GameHandler
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.InitializeKeys))]
    private static void GameSFD_InitializeKeys_Postfix_ExtraVoteKeys()
    {
        GameSFD.m_keyVotes = [
            Keys.F1,
            Keys.F2,
            Keys.F3,
            Keys.F4,
            Keys.F5,
            Keys.F6,
            Keys.F7,
            Keys.F8,
        ];
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(VirtualKeyboard), nameof(VirtualKeyboard.LoadDefaultKeys))]
    private static void VirtualKeyboard_LoadDefaultKeys_Postfix_ExtraVoteKeys()
    {
        VirtualKeyboard.VOTE_MISC_KEYS =
        [
            41,
            42,
            43,
            44,
            45,
            46,
            47,
            48,
        ];
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EffectHandler), nameof(EffectHandler.CreateEffect))]
    private static bool EffectHandler_CreateEffect_Prefix_PreventConsoleSpam(string effectId)
    {
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.TotalGameUserCount), MethodType.Getter)]
    private static bool GameInfo_Getter_TotalGameUserCount_Prefix_DetectSpectators(GameInfo __instance, ref int __result)
    {
        __result = __instance.GetGameUsers().Count((GameUser gameUser) => gameUser.IsUser && !gameUser.JoinedAsSpectator);

        return false;
    }
}
