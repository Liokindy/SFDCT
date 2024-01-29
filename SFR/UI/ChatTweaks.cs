using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.GUI.Text;
using SFD.MenuControls;
using SFD.States;
using SFDCT.Helper;

namespace SFDCT.UI;

[HarmonyPatch]
internal static class ChatTweaks
{
    private static readonly int m_rowSizeNormal = 10;
    private static readonly int m_rowSizeShowHistory = 16;
    private static readonly int m_rowSizeTyping = 13;

    private static int m_lastMessageIndex = 0;
    private static readonly int m_lastMessagesListMax = 128;
    private static List<string> m_lastMessagesList = new(m_lastMessagesListMax);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameChat), nameof(GameChat.Update))]
    private static void Update()
    {
        if (GameChat.m_chatActive)
        {
            if (GameChat.m_rows != m_rowSizeTyping)
            {
                GameChat.m_rows = m_rowSizeTyping;
            }
            return;
        }

        if (GameChat.m_showChatHistory && GameSFD.Handle?.CurrentState is not State.MainMenu)
        {
            if (GameChat.m_rows != m_rowSizeShowHistory)
            {
                GameChat.m_rows = m_rowSizeShowHistory;
            }
            return;
        }

        if (GameChat.m_rows != m_rowSizeNormal)
        {
            GameChat.m_rows = m_rowSizeNormal;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameChat), nameof(GameChat.KeyPress))]
    private static bool KeyPress(ref bool __result, Keys key, bool isRepeat = false)
    {
        if ((key == Keys.Up || key == Keys.Down) && GameChat.ChatActive && m_lastMessagesList != null && m_lastMessagesList.Any() )
        {
            Client client = GameSFD.Handle.Client;
            if (client != null && client.IsRunning)
            {
                __result = false;

                int listIndex = (m_lastMessagesList.Count - 1) - m_lastMessageIndex;
                string message = m_lastMessagesList.ElementAtOrDefault(listIndex);

                m_lastMessageIndex += (key == Keys.Up ? 1 : -1);
                if (m_lastMessageIndex < 0)
                {
                    m_lastMessageIndex = m_lastMessagesList.Count - 1;
                }
                if (m_lastMessageIndex > m_lastMessagesList.Count - 1)
                {
                    m_lastMessageIndex = 0;
                }

                if (!string.IsNullOrEmpty(message))
                {
                    GameChat.m_textbox.SetText(message);
                }
            }
            return false;
        }

        if (m_lastMessageIndex != 0)
        {
            m_lastMessageIndex = 0;
        }
        return true;
    }

    public static void LogChatMessage(string message)
    {
        if (m_lastMessagesList.LastOrDefault() != message)
        {
            if (m_lastMessagesList.Count >= m_lastMessagesListMax)
            {
                m_lastMessagesList.RemoveAt(0);
            }
            m_lastMessagesList.Add(message);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.GameChat_EnterMessageEvent))]
    private static void GameSFD_Message(string message)
    {
        LogChatMessage(message);
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StateEditorTest), nameof(StateEditorTest.GameChat_EnterMessageEvent))]
    private static void EditorTest_Message(string message)
    {
        LogChatMessage(message);
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(StateGameOffline), nameof(StateGameOffline.GameChat_EnterMessageEvent))]
    private static void Offline_Message(string message)
    {
        LogChatMessage(message);
    }
}