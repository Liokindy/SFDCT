using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SFDCT.Helper;
using SFD;
using SFD.States;
using HarmonyLib;

namespace SFDCT.UI;

[HarmonyPatch]
internal static class GameChatHandler
{
    internal const char MULTIPLE_MESSAGES_SEPARATOR = '|';
    internal const sbyte MULTIPLE_MESSAGES_MAX_COUNT = 8;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameSFD), nameof(GameSFD.GameChat_EnterMessageEvent))]
    private static bool GameSFDGameChat_EnterMessageEvent(GameSFD __instance, string message)
    {
        message = message.Trim();
        List<string> validMultipleMessages = [];
        bool badParse = false;
        
        if (message.Contains(MULTIPLE_MESSAGES_SEPARATOR))
        {
            string[] multipleMessages = message.Split(MULTIPLE_MESSAGES_SEPARATOR);

            for (int i = 0; i < Math.Min(MULTIPLE_MESSAGES_MAX_COUNT, multipleMessages.Length); i++)
            {
                multipleMessages[i] = multipleMessages[i].TrimStart();
                if (multipleMessages[i].StartsWith("/"))
                {
                    validMultipleMessages.Add(multipleMessages[i]);
                }
                else
                {
                    badParse = true;
                    break;
                }
            }
        }

        if (badParse || validMultipleMessages.Count <= 1)
        {
            validMultipleMessages.Clear();
            validMultipleMessages.Add(message);
        }

        Client client = __instance.Client;
        bool clientIsRunning = client != null && client.IsRunning;
        foreach(string validMessage in validMultipleMessages)
        {
            if (clientIsRunning)
            {
                client.SendMessage(MessageType.ChatMessage, validMessage);
                continue;
            }
        
            if (__instance.CurrentState == State.MainMenu)
            {
                if (validMessage.StartsWith("/"))
                {
                    HandleCommandArgs handleCommandArgs = new()
                    {
                        Command = validMessage,
                        UserIdentifier = StateGameOffline.GameInfo.GetLocalGameUserIdentifier(0),
                        Origin = HandleCommandOrigin.User
                    };

                    StateGameOffline.GameInfo.HandleCommand(handleCommandArgs);
                }
                else
                {
                    ChatMessage.Show(validMessage, Color.White);
                }
            }
        }

        return false;
    }
}
