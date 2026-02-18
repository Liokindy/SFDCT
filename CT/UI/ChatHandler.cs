using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SFD;
using SFD.MenuControls;
using SFD.States;
using SFDCT.Configuration;
using SFDCT.Helper;

namespace SFDCT.UI;

[HarmonyPatch]
internal static class ChatHandler
{
    internal static bool GameChatInLobby = false;
    internal static int GameChatInLobbyRequestedRows = 0;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameChat), nameof(GameChat.InLobby))]
    private static void GameChat_Update_Postfix_TrackInLobby(bool value, int requestedRows)
    {
        GameChatInLobby = value;
        GameChatInLobbyRequestedRows = requestedRows == 0 ? value ? 5 : 10 : requestedRows;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameChat), nameof(GameChat.Update))]
    private static void GameChat_Update_Postfix_MiscImprovements(bool __state)
    {
        var desiredChatWidth = SFDCTConfig.Get<int>(CTSettingKey.ChatWidth);
        var desiredChatHeight = SFDCTConfig.Get<int>(CTSettingKey.ChatHeight);
        int desiredChatRows;

        if (GameChatInLobby)
        {
            desiredChatWidth = 428;
            desiredChatRows = GameChatInLobbyRequestedRows;
            desiredChatHeight = desiredChatRows * (int)GameChat.MESSAGE_HEIGHT;
        }
        else
        {
            if (GameChat.ChatActive || (!GameChat.ChatActive && Input.KeyDown(GameChat.m_showChatKey)))
            {
                desiredChatHeight += SFDCTConfig.Get<int>(CTSettingKey.ChatExtraHeight);
            }

            desiredChatRows = desiredChatHeight / (int)GameChat.MESSAGE_HEIGHT;
        }

        if (GameChat.m_width != desiredChatWidth)
        {
            GameChat.m_width = desiredChatWidth;
            GameChat.m_textbox.Width = desiredChatWidth;
            GameChat.m_textbox.Label.Width = desiredChatWidth - 2;
        }

        if (GameChat.m_rows != desiredChatRows)
        {
            GameChat.m_rows = desiredChatRows;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameChat), nameof(GameChat.Draw), [typeof(SpriteBatch), typeof(double), typeof(Vector2), typeof(Focus), typeof(bool)])]
    private static bool GameChat_Prefix_Draw_OverrideDraw(SpriteBatch spriteBatch, double elapsedGameTime, Vector2 offset, Focus focus, bool drawBackground)
    {
        if (Constants.FontInGameChat == null)
        {
            GameChat.ChatActive = false;
            return false;
        }

        var elapsed = (float)elapsedGameTime;
        offset.X -= 2;

        if (GameChat.ChatActive)
        {
            if (drawBackground)
            {
                var backgroundWidth = GameChat.m_width + 10;
                var backgroundHeight = GameChat.m_rows * (int)GameChat.MESSAGE_HEIGHT + GameChat.m_textbox.Height + 16;
                var backgroundX = (int)offset.X + ((int)GameChat.LEFT_MARGIN / 2);
                var backgroundY = GameSFD.GAME_HEIGHT + (int)offset.Y - backgroundHeight - 4;
                var backgroundRectangle = new Rectangle(backgroundX, backgroundY, backgroundWidth, backgroundHeight);

                spriteBatch.Draw(Constants.WhitePixel, backgroundRectangle, Constants.COLORS.MENU_BLACK_ALPHA);
            }

            GameChat.m_textbox.LocalPosition += offset;
            GameChat.m_textbox.Draw(spriteBatch, elapsed);

            GameChat.m_textbox.LocalPosition -= offset;
            GameChat.m_textbox.SetFocus(focus);
        }

        if (GameSFD.GUIMode != ShowGUIMode.All && !GameChat.m_showChatHistory && GameSFD.Handle.CurrentState != State.MainMenu)
        {
            return false;
        }

        if (!GameChat.ChatActive && GameChat.m_scroll > 0)
        {
            GameChat.m_chatIconTimer += elapsed;

            if (GameChat.m_chatIconTimer > 250f)
            {
                GameChat.m_chatIconFrame = (GameChat.m_chatIconFrame + 1) % 4;
                GameChat.m_chatIconTimer = 0;
            }

            var chatIconWidth = Constants.ChatIcon.Width;
            var chatIconHeight = Constants.ChatIcon.Height;

            var chatIconUV = new Rectangle(GameChat.m_chatIconFrame * (chatIconWidth / 4), 0, chatIconHeight, chatIconHeight);
            var chatIconRectangle = new Rectangle((int)offset.X + 12, GameSFD.GAME_HEIGHT - 32 + (int)offset.Y, 32, 32);

            spriteBatch.Draw(Constants.ChatIcon, chatIconRectangle, chatIconUV, Constants.COLORS.CHAT_ICON);
        }

        var messageCount = GameChat.m_messages.Count;
        if (messageCount > GameChat.m_scroll + GameChat.m_rows)
        {
            messageCount = GameChat.m_scroll + GameChat.m_rows;
        }

        for (int i = GameChat.m_scroll; i < messageCount; i++)
        {
            if (GameChat.m_messages[i].Time <= 0f && !GameChat.m_showChatHistory && !GameChat.ChatActive)
            {
                return false;
            }

            var messageX = offset.X + GameChat.LEFT_MARGIN;
            var messageY = GameSFD.GAME_HEIGHTf + offset.Y - ((i - GameChat.m_scroll) * GameChat.MESSAGE_HEIGHT + GameChat.BOTTOM_MARGIN) + 2f;
            var messagePosition = new Vector2(messageX, messageY);
            GameChat.m_messages[i].TextInfo.Draw(spriteBatch, messagePosition);
        }

        return false;
    }
}
