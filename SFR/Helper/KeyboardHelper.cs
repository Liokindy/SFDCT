using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFD;
using HarmonyLib;
using Microsoft.Xna.Framework.Input;

namespace SFDCT.Helper;

[HarmonyPatch]
internal static class KeyboardHelper
{
    public static bool IsLeftCtrlDown
    {
        get
        {
            return m_lctrlState == true;
        }
    }
    public static bool IsLeftShiftDown
    {
        get
        {
            return m_lshiftState == true;
        }
    }
    public static bool IsLeftAltDown
    {
        get
        {
            return m_laltState == true;
        }
    }

    private static bool m_lshiftState = false;
    private static bool m_lctrlState = false;
    private static bool m_laltState = false;

    private static void CheckKeys(bool state, Keys key)
    {
        switch(key)
        {
            case Keys.LeftControl:
                m_lctrlState = state;
                break;
            case Keys.LeftShift:
                m_lshiftState = state;
                break;
            case Keys.LeftAlt:
                m_laltState = state;
                break;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateKeyDownEvent))]
    private static void GameSFD_StateKeyDownEvent(Keys key)
    {
        CheckKeys(true, key);
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateKeyUpEvent))]
    private static void GameSFD_StateKeyUpEvent(Keys key)
    {
        CheckKeys(false, key);
    }
}
