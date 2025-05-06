using Microsoft.Xna.Framework.Input;
using SFD;
using HarmonyLib;

namespace SFDCT.Helper;

[HarmonyPatch]
internal static class Keyboard
{
    public static bool IsLeftCtrlDown { get { return KeyDown(Keys.LeftControl); } }
    public static bool IsRightCtrlDown { get { return KeyDown(Keys.RightControl); } }

    public static bool KeyDown(Keys key)
    {
        if (((ushort)key) < m_keyStates.Length)
        {
            return m_keyStates[(ushort)key];
        }
        return false;
    }

    private static readonly bool[] m_keyStates = new bool[256];
    private static void CheckKey(bool state, Keys key)
    {
        if (((ushort)key) < m_keyStates.Length)
        {
            m_keyStates[(ushort)key] = state;
        }
        else
        {
            Logger.LogError("KEY OUTSIDE KEY STATES LENGTH: " + key.ToString());
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateKeyDownEvent))]
    private static void GameSFD_StateKeyDownEvent(Keys key)
    {
        CheckKey(true, key);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateKeyUpEvent))]
    private static void GameSFD_StateKeyUpEvent(Keys key)
    {
        CheckKey(false, key);
    }
}
