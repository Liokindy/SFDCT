using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using SFD;

namespace SFDCT.Helper;

[HarmonyPatch]
internal static class Input
{
    internal static bool IsLeftCtrlDown { get { return KeyDown(Keys.LeftControl); } }
    internal static bool IsRightCtrlDown { get { return KeyDown(Keys.RightControl); } }
    internal static bool IsMouseLeftButtonDown { get { return m_mouseLeftButtonState; } }

    internal static bool KeyDown(Keys key)
    {
        if (((ushort)key) < m_keyStates.Length)
        {
            return m_keyStates[(ushort)key];
        }

        return false;
    }

    private static readonly bool[] m_keyStates = new bool[256];
    private static bool m_mouseLeftButtonState = false;

    private static void PressKey(bool state, Keys key)
    {
        if (((ushort)key) < m_keyStates.Length)
        {
            m_keyStates[(ushort)key] = state;
        }
    }

    private static void PressLeftMouseButton(bool state)
    {
        m_mouseLeftButtonState = state;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateKeyDownEvent))]
    private static void GameSFD_StateKeyDownEvent_Postfix_InputCheck(Keys key)
    {
        PressKey(true, key);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateKeyUpEvent))]
    private static void GameSFD_StateKeyUpEvent_Postfix_InputCheck(Keys key)
    {
        PressKey(false, key);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateMouseLeftDownEvent))]
    private static void GameSFD_StateMouseLeftDownEvent_Postfix_InputCheck()
    {
        PressLeftMouseButton(true);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateMouseLeftUpEvent))]
    private static void GameSFD_StateMouseLeftUpEvent_Postfix_InputCheck()
    {
        PressLeftMouseButton(false);
    }
}
