//using System.Collections.Generic;
//using System.Linq;
//using System.Windows.Forms;
//using HarmonyLib;
//using SFD;
//using SFD.States;

//namespace SFDCT.UI;

//[HarmonyPatch]
//internal static class ChatHandler
//{
//    private static int m_lastMessageIndex = 0;
//    private static List<string> m_lastMessagesList = new(16);

//    [HarmonyPrefix]
//    [HarmonyPatch(typeof(GameChat), nameof(GameChat.KeyPress))]
//    private static bool KeyPress(ref bool __result, Keys key, bool isRepeat = false)
//    {
//        if (SFD.GameChat.ChatActive)
//        {
//            if ((key == Keys.Up || key == Keys.Down) && m_lastMessagesList != null && m_lastMessagesList.Any() )
//            {
//                __result = false;

//                int listIndex = (m_lastMessagesList.Count - 1) - m_lastMessageIndex;
//                string message = m_lastMessagesList.ElementAtOrDefault(listIndex);

//                m_lastMessageIndex += (key == Keys.Up ? 1 : -1);
//                if (m_lastMessageIndex < 0)
//                {
//                    m_lastMessageIndex = m_lastMessagesList.Count - 1;
//                }
//                if (m_lastMessageIndex > m_lastMessagesList.Count - 1)
//                {
//                    m_lastMessageIndex = 0;
//                }

//                if (!string.IsNullOrEmpty(message))
//                {
//                    SFD.GameChat.m_textbox.SetText(message);
//                }
//                return false;
//            }
//        }

//        if (m_lastMessageIndex != 0)
//        {
//            m_lastMessageIndex = 0;
//        }
//        return true;
//    }

//    private static void LogChatMessage(string message)
//    {
//        if (m_lastMessagesList.LastOrDefault() != message)
//        {
//            if (m_lastMessagesList.Count >= 16)
//            {
//                m_lastMessagesList.RemoveAt(0);
//            }
//            m_lastMessagesList.Add(message);
//        }
//    }


//    [HarmonyPostfix]
//    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.GameChat_EnterMessageEvent))]
//    private static void GameSFD_Message(string message)
//    {
//        LogChatMessage(message);
//    }
//    [HarmonyPostfix]
//    [HarmonyPatch(typeof(StateEditorTest), nameof(StateEditorTest.GameChat_EnterMessageEvent))]
//    private static void EditorTest_Message(string message)
//    {
//        LogChatMessage(message);
//    }
//    [HarmonyPostfix]
//    [HarmonyPatch(typeof(StateGameOffline), nameof(StateGameOffline.GameChat_EnterMessageEvent))]
//    private static void Offline_Message(string message)
//    {
//        LogChatMessage(message);
//    }
//}