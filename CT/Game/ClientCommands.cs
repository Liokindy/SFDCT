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
                if (gameUser.GameSlotIndex == -1) messageColor *= 0.8f;

                messageArgs =
                [
                    gameUser.GameSlotIndex == -1 ? "#" : gameUser.GameSlotIndex.ToString(),
                    gameUser.GetProfileName(),
                    gameUser.AccountName,
                    (gameUser.IsHost ? "HOST" : gameUser.IsModerator ? "MOD" : "") + (gameUser.JoinedAsSpectator ? " SPECTATOR" : ""),
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
