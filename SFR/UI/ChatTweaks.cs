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
using CConst = SFDCT.Misc.Constants;

namespace SFDCT.UI;

[HarmonyPatch]
internal static class ChatTweaks
{
    private static int m_rowSizeNormal = 10;
    private static int m_rowSizeShowHistory = 13;
    private static int m_rowSizeTyping = 15;
    private static bool IsWordSeparator(char c)
    {
        return c != '_' && !((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'));
    }

    private static int m_lastMessageIndex = 0;
    private static int m_lastMessagesListMax = 128;
    private static List<string> m_lastMessagesList = new(m_lastMessagesListMax);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameChat), nameof(GameChat.Update))]
    private static void Update()
    {
        if (GameChat.m_chatActive)
        {
            // Staff-chat
            if (!string.IsNullOrEmpty(GameChat.m_textbox.Text))
            {
                if (GameChat.m_textbox.Text.StartsWith("/staff ") || GameChat.m_textbox.Text == "/staff" ||
                    GameChat.m_textbox.Text.StartsWith("/s ") || GameChat.m_textbox.Text == "/s")
                {
                    Client client = GameSFD.Handle.Client;
                    GameInfo gameInfo;
                    if (client != null && client.IsRunning && client.GameInfo != null)
                    {
                        gameInfo = GameSFD.Handle.Client.GameInfo;
                    }
                    else
                    {
                        gameInfo = GameSFD.Handle.GetRunningState().GetActiveGameInfo();
                    }

                    GameUser gameUser = gameInfo?.GetLocalGameUser(0);
                    if (gameUser != null && (gameUser.IsModerator || gameUser.IsHost))
                    {
                        GameChat.m_textbox.LabelColor = CConst.Colors.Staff_Chat_Message;
                    }
                }
            }

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
        if (GameChat.ChatActive)
        {
            if ((key == Keys.Up || key == Keys.Down) && m_lastMessagesList != null && m_lastMessagesList.Any() )
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
            if (key == Keys.Back && GameChat.m_textbox.Text.Length > 0)
            {
                bool ctrlHeld = false;
                if (GameSFD.Handle.GetRunningState() != null && GameSFD.Handle.GetRunningState().GetActiveGameWorld() != null && !GameSFD.Handle.GetRunningState().GetActiveGameWorld().IsDisposed)
                {
                    ctrlHeld = GameSFD.Handle.GetRunningState().GetActiveGameWorld().IsControlPressed();
                }

                if (ctrlHeld)
                {
                    string textboxText = GameChat.m_textbox.Text;
                    int charCount = 0;

                    for(int i = textboxText.Length; i > 0; i--)
                    {
                        char currentChar = textboxText[i - 1];
                        bool isWordSep = IsWordSeparator(currentChar);
                        if (isWordSep && IsWordSeparator(textboxText.ElementAtOrDefault(i - 2)))
                        {
                            continue;
                        }

                        if (isWordSep && i != textboxText.Length)
                        {
                            charCount = i + 1;
                            break;
                        }
                    }
                    charCount = Math.Max(Math.Min(charCount, textboxText.Length), 0);

                    GameChat.m_textbox.SetText(textboxText.Substring(0, charCount));
                }
            }
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

    public static string ConstructMetaTextToStaffChatMessage(string msg, string plrName, TeamIcon team)
    {
        string text = string.Format("[{0}]{1}:[#] [{2}]{3}",
        [
            CConst.Colors.Staff_Chat_Name.ToHex(),
            TextMeta.EscapeText(plrName),
            CConst.Colors.Staff_Chat_Message.ToHex(),
            TextMeta.EscapeText(msg)
        ]);
        text = string.Format("[ICO=TEAM_{0}]{1}", (int)team, text);
        return string.Format("[{0}]{1}[#]{2}", CConst.Colors.Staff_Chat_Tag.ToHex(), TextMeta.EscapeText("[To staff]"), text);
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