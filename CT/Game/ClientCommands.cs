using Microsoft.Xna.Framework;
using SFD;
using System.Linq;

namespace SFDCT.Game;

internal static class ClientCommands
{
    internal static bool HandleClearChat(Client server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        GameChat.ClearChat();
        return true;
    }

    internal static bool HandleListPlayers(Client server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        int gameUserCount = 0;

        foreach (GameUser gameUser in gameInfo.GetGameUsers().OrderBy(g => g.GameSlotIndex))
        {
            if (gameUser.IsDedicatedPreview && !GameSFD.Handle.ImHosting) continue;

            var messageKey = gameUser.IsBot ? "sfdct.command.players.message.bot" : "sfdct.command.players.message.user";
            var messageColor = gameUser.IsHost ? Color.LightPink : gameUser.IsModerator ? Color.LightGreen : Color.LightBlue;
            string[] messageArgs;

            messageColor *= (gameUserCount % 2 == 0) ? 0.8f : 0.9f;

            if (gameUser.IsBot)
            {
                messageArgs =
                [
                    gameUser.GameSlotIndex.ToString(),
                        gameUser.GetProfileName(),
                    ];
            }
            else
            {
                bool noSlot = gameUser.JoinedAsSpectator || gameUser.GameSlotIndex == -1;

                if (noSlot) messageColor *= 0.5f;

                string gameUserAccountName;
                if (!gameInfo.AccountNameInfo.TryGetAccountName(gameUser.UserIdentifier, out gameUserAccountName))
                {
                    gameUserAccountName = "#" + gameUser.UserIdentifier.ToString();
                }

                messageArgs =
                [
                    noSlot ? "#" : gameUser.GameSlotIndex.ToString(),
                    gameUser.GetProfileName(),
                    gameUser.IsBot ? Profile.DEFAULT_BOT_NAME : gameUserAccountName,
                    gameUser.IsHost ? "HOST" : gameUser.IsModerator ? "MOD" : "",
                ];
            }

            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText(messageKey, messageArgs), messageColor, args.SenderGameUser));

            gameUserCount++;
        }

        return true;
    }

    internal static bool HandleCTHelp(Client client, ProcessCommandArgs args, GameInfo gameInfo)
    {
        var colYellow = Color.Yellow;

        args.Feedback.Add(new(args.SenderGameUser, "'/CLEARCHAT' to clear the chat in your screen.", colYellow, args.SenderGameUser, null));

        // pass the command to the server
        return false;
    }
}
