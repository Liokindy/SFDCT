using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using SFD;
using SFD.GameKeyboard;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class GameHandler
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.InitializeKeys))]
    private static void InitializeKeys()
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
    private static void LoadDefaultKeys()
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
}
