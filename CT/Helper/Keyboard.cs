//using SFD;
//using HarmonyLib;
//using Microsoft.Xna.Framework.Input;

//namespace SFDCT.Helper;

//[HarmonyPatch]
//internal static class Keyboard
//{
//    public static bool IsLeftCtrlDown
//    {
//        get
//        {
//            return m_lctrlState;
//        }
//    }
//    public static bool IsRightCtrlDown
//    {
//        get
//        {
//            return m_rctrlState;
//        }
//    }
//    public static bool IsLeftShiftDown
//    {
//        get
//        {
//            return m_lshiftState;
//        }
//    }
//    public static bool IsLeftAltDown
//    {
//        get
//        {
//            return m_laltState;
//        }
//    }
//    public static bool IsRightAltDown
//    {
//        get
//        {
//            return m_raltState;
//        }
//    }

//    private static bool m_lshiftState = false;
//    private static bool m_lctrlState = false;
//    private static bool m_rctrlState = false;
//    private static bool m_laltState = false;
//    private static bool m_raltState = false;

//    private static void CheckKeys(bool state, Keys key)
//    {
//        switch(key)
//        {
//            case Keys.LeftControl:
//                m_lctrlState = state;
//                break;
//            case Keys.RightControl:
//                m_rctrlState = state;
//                break;
//            case Keys.LeftShift:
//                m_lshiftState = state;
//                break;
//            case Keys.LeftAlt:
//                m_laltState = state;
//                break;
//            case Keys.RightAlt:
//                m_raltState = state;
//                break;
//        }
//    }

//    [HarmonyPostfix]
//    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateKeyDownEvent))]
//    private static void GameSFD_StateKeyDownEvent(Keys key)
//    {
//        CheckKeys(true, key);
//    }
//    [HarmonyPostfix]
//    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateKeyUpEvent))]
//    private static void GameSFD_StateKeyUpEvent(Keys key)
//    {
//        CheckKeys(false, key);
//    }
//}
