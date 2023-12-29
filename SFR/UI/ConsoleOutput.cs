using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.MapEditor;
using SFR.Helper;
using System.Reflection;

namespace SFR.UI;

/// <summary>
///     A new Console Output ( F11 )
/// </summary>
[HarmonyPatch]
internal static class NewConsoleOutput
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.StateMouseScrollEvent))]
    public static void StateMouseScrollEvent(Microsoft.Xna.Framework.Rectangle mouseSelection, int scrollValue)
    {
        if (ConsoleOutput.IsVisible)
        {
            Scroll(scrollValue);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch( typeof(ConsoleOutput), nameof(ConsoleOutput.Init) )]
    private static bool _Init()
    {
        Init();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ConsoleOutput), nameof(ConsoleOutput.GetLatestMessages))]
    public static bool _GetLatestMessages(string __result)
    {
        __result = GetLatestMessages();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ConsoleOutput), nameof(ConsoleOutput.ShowMessage))]
    public static bool _ShowMessage(ConsoleOutputType msgType, string msg)
    {
        ShowMessage(msgType, msg);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ConsoleOutput), nameof(ConsoleOutput.Draw))]
    public static bool _Draw(SpriteBatch spriteBatch)
    {
        Draw(spriteBatch);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ConsoleOutput), nameof(ConsoleOutput.Hide))]
    private static bool _Hide()
    {
        Hide();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ConsoleOutput), nameof(ConsoleOutput.Show))]
    private static bool _Show()
    {
        Show();
        return false;
    }

    private static bool m_finishedAnimation = true;
    private static int m_scroll = 0;
    private static int m_textsVisibleAmount = 24;
    private static int m_textsLoggedAmount = 512;
    private static float m_drawY = -ConsoleOutput.m_textHeight * m_textsVisibleAmount;
    private static float m_animationSpeed = 16f;
    private static Color m_separatorColor = new Color(1f, 1f, 1f, 0.5f);
    private static Color m_backgroundColor = new Color(0, 0, 0, 0.5f);
    private static List<ConsoleOutput.ConsoleText> m_texts = new List<ConsoleOutput.ConsoleText>();

    private static void Hide()
    {
        ConsoleOutput.IsVisible = false;
        m_finishedAnimation = false;
    }
    private static void Show()
    {
        ConsoleOutput.IsVisible = true;
        m_finishedAnimation = false;
        m_scroll = 0;
    }

    /// <summary>
    ///     Gets the last 20 messages of the ConsoleOutput.
    ///     Used by the crash report.
    /// </summary>
    private static string GetLatestMessages()
    {
        string text = "";
        lock (ConsoleOutput.m_lock)
        {
            List<ConsoleOutput.ConsoleText> msgList = GetLastMessages(0, 20);
            foreach( var cText in msgList )
            {
                text += cText.Message + "\r\n";
            }
        }
        return text;
    }
    
    /// <summary>
    ///     Initializes the value of the console messages
    /// </summary>
    private static void Init()
    {
        ConsoleOutput.IsVisible = false;
        m_texts = new List<ConsoleOutput.ConsoleText>();
    }
    
    /// <summary>
    ///     Adds a message to the console output
    /// </summary>
    private static void ShowMessage(ConsoleOutputType msgType, string msg)
    {
        if (msgType == ConsoleOutputType.Script && !ConsoleOutput.IsVisible && GameSFD.Handle.CurrentState != SFD.States.State.EditorTestRun)
        {
            return;
        }
        if (msgType == ConsoleOutputType.TextureLookup || msgType == ConsoleOutputType.TileLookup)
        {
            return;
        }

        ConsoleOutput.m_messageCounter += 1;
        string text = string.Format("{0:000000}-{1:00}: {2}", ConsoleOutput.m_messageCounter, (int)msgType, msg);

        ConsoleOutput.ConsoleText consoleText = new ConsoleOutput.ConsoleText();
        if (msgType == ConsoleOutputType.Script)
        {
            if (GameSFD.Handle.CurrentState == SFD.States.State.EditorTestRun && SFDMapEditor.MapDebugForm.Form != null)
            {
                Task.Factory.StartNew(delegate ()
                {
                    SFDMapEditor.MapDebugForm.Form.LogOutput(msg);
                }).Wait();
            }
            text = "Script: " + text;
        }
        consoleText.SetText(10000.0, ConsoleOutput.MessageTypeToColor(msgType), msgType, text);
        m_texts.Add(consoleText);
        
        if (m_texts.Count > m_textsLoggedAmount)
        {
            m_texts.RemoveAt(0);
            if (m_scroll > m_texts.Count)
            {
                m_scroll = m_texts.Count;
            }
        }
    }

    private static void Draw(SpriteBatch spriteBatch)
    {
        if (!m_finishedAnimation)
        {
            if (ConsoleOutput.IsVisible)
            {
                m_drawY += m_animationSpeed;
                if (m_drawY > 0)
                {
                    m_drawY = 0;
                    m_finishedAnimation = true;
                }
            }
            else
            { 
                m_drawY -= m_animationSpeed;
                if (m_drawY < ConsoleOutput.m_textHeight * -m_textsVisibleAmount)
                {
                    m_drawY = ConsoleOutput.m_textHeight * -m_textsVisibleAmount;
                    m_finishedAnimation = true;
                }
            }
        }

        if (ConsoleOutput.IsVisible || !m_finishedAnimation)
        {
            spriteBatch.Draw(SFD.Constants.WhitePixel, new Rectangle(0, (int)m_drawY, GameSFD.GAME_WIDTH, (int)(m_textsVisibleAmount*ConsoleOutput.m_textHeight)), m_backgroundColor);

            float num = m_drawY;
            foreach( var text in GetLastMessages(m_scroll, m_textsVisibleAmount) )
            {
                ConsoleOutput.DrawText(spriteBatch, num, text.Message, text.MessageColor);
                num += ConsoleOutput.m_textHeight;
            }

            spriteBatch.Draw(SFD.Constants.WhitePixel, new Rectangle(0, (int)num, GameSFD.GAME_WIDTH, 2), m_separatorColor);
        }
    }
    private static void Scroll(int scrollValue)
    {
        if (scrollValue > 0)
        {
            m_scroll++;
        }
        else
        {
            m_scroll--;
        }

        if (m_scroll < 0)
        {
            m_scroll = 0;
        }
        if (m_scroll > m_texts.Count)
        {
            m_scroll = m_texts.Count;
        }
    }

    private static List<ConsoleOutput.ConsoleText> GetLastMessages(int indexOffset, int msgCount)
    {
        List<ConsoleOutput.ConsoleText> msgList = new List<ConsoleOutput.ConsoleText>();

        if (m_texts.Count <= msgCount)
        {
            msgList = m_texts.ToList();
        }
        else
        {
            int index = m_texts.Count - indexOffset - msgCount;
            if (index < 0)
            {
                index = 0;
            }
            if (index+msgCount > m_texts.Count)
            {
                index = m_texts.Count - msgCount;
            }
            msgList = m_texts.GetRange(index, msgCount);
        }


        return msgList;
    }
}
