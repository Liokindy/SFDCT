using Microsoft.Xna.Framework.Input;
using SFD;
using HarmonyLib;

namespace SFDCT.Helper;

[HarmonyPatch]
internal static class Input
{
    public static bool IsLeftCtrlDown { get { return KeyDown(Keys.LeftControl); } }
    public static bool IsRightCtrlDown { get { return KeyDown(Keys.RightControl); } }
    public static bool IsMouseLeftButtonDown { get { return m_mouseLeftButtonState; } }

    public static bool KeyDown(Keys key)
    {
        if (((ushort)key) < m_keyStates.Length)
        {
            return m_keyStates[(ushort)key];
        }
        return false;
    }

    private static readonly bool[] m_keyStates = new bool[256];
    private static bool m_mouseLeftButtonState = false;
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
    private static void CheckMouse(bool state)
    {
        m_mouseLeftButtonState = state;
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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateMouseLeftDownEvent))]
    private static void GameSFD_StateMouseLeftDownEvent()
    {
        CheckMouse(true);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateMouseLeftUpEvent))]
    private static void GameSFD_StateMouseLeftUpEvent()
    {
        CheckMouse(false);
    }
}
