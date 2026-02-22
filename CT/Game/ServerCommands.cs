using Lidgren.Network;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Core;
using SFD.GUI.Text;
using SFD.Parser;
using SFD.Sounds;
using SFD.Voting;
using SFDCT.Configuration;
using SFDCT.Sync;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFDCT.Game;

internal static class ServerCommands
{
    internal static bool HandleExec(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        CommandHandler.ExecuteCommandsFile(ref args, gameInfo, args.SourceParameters);
        return true;
    }

    internal static bool HandleGravity(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        var defaultGravity = new Vector2(0, -26);
        if (args.Parameters.Count < 2)
        {
            gameInfo.GameWorld.GetActiveWorld.Gravity = defaultGravity;
            gameInfo.GameWorld.GetBackgroundWorld.Gravity = defaultGravity;
            return true;
        }

        float gravityX, gravityY;
        if (!SFDXParser.TryParseFloat(args.Parameters[0], out gravityX)) gravityX = defaultGravity.X;
        if (!SFDXParser.TryParseFloat(args.Parameters[1], out gravityY)) gravityX = defaultGravity.Y;

        var newGravity = new Vector2(gravityX, gravityY);

        gameInfo.GameWorld.GetActiveWorld.Gravity = newGravity;
        gameInfo.GameWorld.GetBackgroundWorld.Gravity = newGravity;

        args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.gravity.message", newGravity.ToString())));
        return true;
    }

    internal static bool HandleMeta(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        if (string.IsNullOrEmpty(args.SourceParameters))
        {
            string exampleMetaText = "- Default. [#FF00FF]Magenta[#]. [#FF0]Yellow[#]. Icon [ICO=TEAM_1]";

            args.Feedback.Add(new(args.SenderGameUser, "Meta-formatting allows to specify text color ('[#FFFFFF]'), reset text color ('[#]'), and display icons ('[ICO=]'). Example:", args.SenderGameUser));
            args.Feedback.Add(new(args.SenderGameUser, exampleMetaText, true, Constants.COLORS.LIGHT_GRAY, args.SenderGameUser));
            args.Feedback.Add(new(args.SenderGameUser, exampleMetaText, false, Constants.COLORS.LIGHT_GRAY, args.SenderGameUser));

            var availableIcons = TextIcons.m_icons.Keys.Select(iconKey => $"{TextMeta.EscapeText(iconKey)} ([ICO={iconKey}])");
            string availableIconsText = "- " + string.Join(", ", availableIcons);

            args.Feedback.Add(new(args.SenderGameUser, "Available Icons:", args.SenderGameUser));
            args.Feedback.Add(new(args.SenderGameUser, availableIconsText, true, Constants.COLORS.LIGHT_GRAY, args.SenderGameUser));

            return true;
        }

        var message = args.SourceParameters;
        var chatMessageData = new NetMessage.ChatMessage.Data(message, Color.White, args.SenderGameUser.GetProfileName(), true, args.SenderGameUser.UserIdentifier);

        if (gameInfo.GameOwner == GameOwnerEnum.Server)
        {
            server.SendMessage(MessageType.ChatMessage, chatMessageData);
        }
        else
        {
            gameInfo.ShowChatMessage(chatMessageData);
        }

        return true;
    }

    internal static bool HandleModCommands(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        var action = args.Parameters.ElementAtOrDefault(0).ToUpperInvariant();
        var commands = new List<string>();

        if (args.Parameters.Count >= 2) commands = args.Parameters.GetRange(1, args.Parameters.Count - 1);
        if (args.Parameters.Count == 2 && args.Parameters[1] == "*") commands = GameInfo.ALL_MODERATOR_COMMANDS.ToList();

        string header1, header2, message1;
        var colLightGreen = new Color(159, 255, 64);
        var shouldSave = false;

        switch (action)
        {
            default:
                header1 = LanguageHelper.GetText("sfdct.command.modcommands.header.help");

                args.Feedback.Add(new(args.SenderGameUser, header1, colLightGreen, args.SenderGameUser));
                break;
            case "L":
            case "LIST":
                header1 = LanguageHelper.GetText("sfdct.command.modcommands.header.list");
                message1 = "- {0}";

                args.Feedback.Add(new(args.SenderGameUser, header1, colLightGreen, args.SenderGameUser));
                args.Feedback.Add(new(args.SenderGameUser, string.Format(message1, string.Join(" ", Constants.MODDERATOR_COMMANDS)), colLightGreen * 0.5f, args.SenderGameUser));
                break;
            case "C":
            case "CLEAR":
                header1 = LanguageHelper.GetText("sfdct.command.modcommands.header.clear");

                int clearedCount = 0;
                shouldSave = true;

                clearedCount += Constants.MODDERATOR_COMMANDS.Count;
                Constants.MODDERATOR_COMMANDS.Clear();

                args.Feedback.Add(new(args.SenderGameUser, string.Format(header1, clearedCount), colLightGreen, args.SenderGameUser));
                break;
            case "R":
            case "REMOVE":
                if (commands.Count > 0)
                {
                    header1 = LanguageHelper.GetText("sfdct.command.modcommands.header.remove");
                    message1 = "- {0}";

                    int removedCount = 0;
                    shouldSave = true;

                    foreach (string modderatorCommand in Constants.MODDERATOR_COMMANDS.ToList())
                    {
                        if (commands.Contains(modderatorCommand, StringComparer.OrdinalIgnoreCase))
                        {
                            if (Constants.MODDERATOR_COMMANDS.Remove(modderatorCommand))
                            {
                                removedCount++;
                            }
                        }
                    }

                    args.Feedback.Add(new(args.SenderGameUser, string.Format(header1, removedCount), colLightGreen, args.SenderGameUser));
                    if (removedCount > 0)
                    {
                        args.Feedback.Add(new(args.SenderGameUser, string.Format(message1, string.Join(" ", commands)), colLightGreen * 0.5f, args.SenderGameUser));
                    }
                }
                break;
            case "A":
            case "ADD":
                if (commands.Count > 0)
                {
                    header1 = LanguageHelper.GetText("sfdct.command.modcommands.header.add");
                    message1 = "- {0}";

                    int addedCount = 0;
                    shouldSave = true;

                    foreach (string command in commands)
                    {
                        if (!Constants.MODDERATOR_COMMANDS.Contains(command, StringComparer.OrdinalIgnoreCase))
                        {
                            Constants.MODDERATOR_COMMANDS.Add(command.ToUpperInvariant());
                            addedCount++;
                        }
                    }

                    args.Feedback.Add(new(args.SenderGameUser, string.Format(header1, addedCount), colLightGreen, args.SenderGameUser));
                    if (addedCount > 0)
                    {
                        args.Feedback.Add(new(args.SenderGameUser, string.Format(message1, string.Join(" ", commands)), colLightGreen * 0.5f, args.SenderGameUser));
                    }
                }
                break;
            case "T":
            case "TRY":
                if (commands.Count > 0)
                {
                    header1 = LanguageHelper.GetText("sfdct.command.modcommands.header.try.true");
                    header2 = LanguageHelper.GetText("sfdct.command.modcommands.header.try.false");
                    message1 = "- {0}";

                    List<string> canUseList = [];
                    List<string> canNotUseList = [];

                    foreach (string command in commands)
                    {
                        if (Constants.MODDERATOR_COMMANDS.Count > 0 && !Constants.MODDERATOR_COMMANDS.Contains(command))
                        {
                            canNotUseList.Add(command);
                        }
                        else
                        {
                            canUseList.Add(command);
                        }
                    }

                    args.Feedback.Add(new(args.SenderGameUser, string.Format(header2), colLightGreen, args.SenderGameUser));
                    if (canNotUseList.Count > 0)
                    {
                        args.Feedback.Add(new(args.SenderGameUser, string.Format(message1, string.Join(" ", canNotUseList)), colLightGreen * 0.5f, args.SenderGameUser));
                    }
                    args.Feedback.Add(new(args.SenderGameUser, string.Format(header1), colLightGreen, args.SenderGameUser));

                    if (canUseList.Count > 0)
                    {
                        args.Feedback.Add(new(args.SenderGameUser, string.Format(message1, string.Join(" ", canUseList)), colLightGreen * 0.5f, args.SenderGameUser));
                    }
                }
                break;
        }

        if (shouldSave) SFDConfig.SaveConfig(SFDConfigHandler.SAVE_MODERATOR_COMMANDS);

        return true;
    }

    internal static bool HandleHurt(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        if (args.Parameters.Count < 2) return true;

        GameUser user = gameInfo.GetGameUserByStringInput(args.Parameters[0], args.SenderGameUser);
        if (user == null || user.IsDisposed) return true;

        Player userPlayer = gameInfo.GameWorld.GetPlayerByUserIdentifier(user.UserIdentifier);
        if (userPlayer == null || userPlayer.IsDisposed) return true;

        var damage = 0f;
        if (!SFDXParser.TryParseFloat(args.Parameters[1], out damage)) return true;

        if (damage > 0f)
        {
            userPlayer.TakeMiscDamage(damage, false);
        }
        else if (damage < 0f)
        {
            userPlayer.HealAmount(-damage);
        }

        string message = LanguageHelper.GetText("sfdct.command.damage.message", damage.ToString(), user.GetProfileName());
        args.Feedback.Add(new(args.SenderGameUser, message));

        return true;
    }

    internal static bool HandleDebugMouse(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        if (args.Parameters.Count < 1) return true;

        var messageKey = "sfdct.command.debugmouse.message";

        if (args.Parameters[0] == "0" || args.Parameters[0].ToUpperInvariant() == "FALSE")
        {
            ServerHandler.DebugMouse = false;

            string message = LanguageHelper.GetText(messageKey, LanguageHelper.GetBooleanText(false));
            args.Feedback.Add(new(args.SenderGameUser, message));
        }
        else if (args.Parameters[0] == "1" || args.Parameters[0].ToUpperInvariant() == "TRUE")
        {
            ServerHandler.DebugMouse = true;

            string message = LanguageHelper.GetText(messageKey, LanguageHelper.GetBooleanText(true));
            args.Feedback.Add(new(args.SenderGameUser, message));
        }

        return true;
    }

    internal static bool HandleServerMovement(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        if (args.Parameters.Count == 0) return true;

        var gameUser = gameInfo.GetGameUserByStringInput(args.Parameters[0], args.SenderGameUser);
        if (gameUser == null || gameUser.IsDisposed || gameUser.IsBot) return true;

        var gameUserTag = gameUser.GetGameConnectionTag();
        if (gameUserTag == null || gameUserTag.IsDisposed || gameUserTag.GameUsers == null) return true;

        var serverMovement = 0;
        if (args.Parameters.Count >= 2)
        {
            int.TryParse(args.Parameters[1], out serverMovement);
        }

        gameUserTag.ForcedServerMovementToggleTime = serverMovement == 1 ? ServerHandler.SERVER_MOVEMENT_TOGGLE_TIME_MS_FORCE_TRUE : serverMovement == 0 ? ServerHandler.SERVER_MOVEMENT_TOGGLE_TIME_MS_FORCE_FALSE : Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_TOGGLE_TIME_MS;
        gameUserTag.ForceServerMovement = serverMovement == 1;

        string messageKey = "sfdct.command.servermovement.message";
        string message = LanguageHelper.GetText(messageKey, gameUser.GetProfileName(), serverMovement == 0 ? LanguageHelper.GetText("properties.script.spawnFire.type.default") : LanguageHelper.GetBooleanText(serverMovement == 2));
        args.Feedback.Add(new(args.SenderGameUser, message, args.SenderGameUser));

        return true;
    }

    internal static bool HandleVoteKick(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.VoteKickEnabled)) return true;
        if (args.Parameters.Count <= 0) return true;

        if (!Voting.GameVoteYesNo.CanStartVote(gameInfo))
        {
            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.fail"), Color.Red, args.SenderGameUser));
            return true;
        }

        GameUser userToKick = gameInfo.GetGameUserByStringInput(args.SourceParameters);

        if (userToKick == null || userToKick.IsDisposed || userToKick == args.SenderGameUser) return true;
        if (userToKick.IsModerator || userToKick.IsBot)
        {
            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.fail.invaliduser"), Color.Red, args.SenderGameUser));
            return true;
        }

        GameUser kickOwnerUser = args.SenderGameUser;

        string userToKickProfileName = userToKick.GetProfileName();
        string userTokickAccountName = userToKick.IsBot ? "COM" : userToKick.AccountName;

        string kickOwnerUserProfileName = kickOwnerUser.GetProfileName();
        string kickOwnerUserAccountName = kickOwnerUser.IsBot ? "COM" : kickOwnerUser.AccountName;

        string soundID = "PlayerLeave";

        long[] validRemoteUniqueIdentifiers = server.GetConnectedUniqueIdentifiers((NetConnection x) => x.GameConnectionTag() != null && x.GameConnectionTag().FirstGameUser != null && x.GameConnectionTag().FirstGameUser.UserIdentifier != userToKick.UserIdentifier && x.GameConnectionTag().FirstGameUser.CanVote);
        if (validRemoteUniqueIdentifiers.Length <= 3)
        {
            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.fail.notenoughusers"), Color.Red, args.SenderGameUser));
            return true;
        }

        ConsoleOutput.ShowMessage(ConsoleOutputType.Information, string.Format("Creating vote-kick from '{0}' ({1}) against '{2}' ({3})", kickOwnerUserProfileName, kickOwnerUserAccountName, userToKickProfileName, userTokickAccountName));

        var vote = new Voting.GameVoteKick(GameVote.GetNextVoteID(), userToKickProfileName, userTokickAccountName, userToKick.GetNetIP());
        vote.ValidRemoteUniqueIdentifiers.AddRange(validRemoteUniqueIdentifiers);

        server.SendMessage(MessageType.GameVote, new Pair<GameVote, bool>(vote, false));
        server.SendMessage(MessageType.Sound, new NetMessage.Sound.Data(soundID, true, Vector2.Zero, 1f));

        gameInfo.VoteInfo.AddVote(vote);
        args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.message", kickOwnerUserProfileName, kickOwnerUserAccountName, userToKickProfileName, userTokickAccountName), Color.Yellow));
        args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.message.victim", kickOwnerUserProfileName, kickOwnerUserAccountName), Color.Yellow * 0.6f, userToKick));
        args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.message.owner", userToKickProfileName, userTokickAccountName), Color.Yellow * 0.6f, args.SenderGameUser));
        return true;
    }

    internal static bool HandleSpectatorJoin(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        if (!args.SenderGameUser.JoinedAsSpectator) return true;

        List<GameSlot> availableGameSlots = null;
        if (gameInfo.GameOwner == GameOwnerEnum.Server)
        {
            var connectionTag = args.SenderGameUser.GetGameConnectionTag();
            if (connectionTag == null) return true;
            if (connectionTag.GameUsers.Length > 1) return true;

            availableGameSlots = server.FindOpenGameSlots(gameInfo.DropInMode, 1, gameInfo.EvenTeams);
        }
        else
        {
            availableGameSlots = [gameInfo.GameSlots[0]];
        }

        if (availableGameSlots == null || availableGameSlots.Count == 0)
        {
            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.join.fail.nogameslot"), Color.Red, args.SenderGameUser));
            return true;
        }

        GameSlot gameSlot = availableGameSlots[0];
        gameSlot.ClearGameUser(gameInfo);
        gameSlot.GameUser = args.SenderGameUser;
        gameSlot.CurrentState = GameSlot.State.Occupied;
        args.SenderGameUser.GameSlot = gameSlot;
        args.SenderGameUser.JoinedAsSpectator = false;
        args.SenderGameUser.SpectatingWhileWaitingToPlay = true;

        var messageKey = "menu.lobby.newPlayerJoined";
        var messageArgs = new List<string>();
        var messageColor = Constants.COLORS.PLAYER_CONNECTED;

        messageArgs.Add(args.SenderGameUser.GetProfileName());
        if (gameSlot.CurrentTeam != Team.Independent)
        {
            messageKey = "menu.lobby.newPlayerJoinedTeam";
            messageArgs.Add(((int)gameSlot.CurrentTeam).ToString());
        }

        if (gameInfo.GameOwner == GameOwnerEnum.Server)
        {
            server.SendMessage(MessageType.ChatMessage, new NetMessage.ChatMessage.Data(messageKey, messageColor, messageArgs.ToArray()));
            server.SendMessage(MessageType.Sound, new NetMessage.Sound.Data("PlayerJoin", true, Vector2.Zero, 1f));
            server.SyncGameUserInfo(args.SenderGameUser);
            server.SyncGameSlotInfo(gameSlot);
        }
        else
        {
            gameInfo.ShowChatMessage(new(messageKey, messageColor, messageArgs.ToArray()));
            SoundHandler.PlayGlobalSound("PlayerJoin");
        }

        args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.join.message"), Color.Gray, args.SenderGameUser));
        return true;
    }

    internal static bool HandleSpectatorSpectate(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        if (args.SenderGameUser.JoinedAsSpectator) return true;
        if (!args.ModeratorPrivileges && SFDCTConfig.Get<bool>(CTSettingKey.SpectatorsOnlyModerators)) return true;
        if (gameInfo.SpectatorGameUserCount >= SFDCTConfig.Get<int>(CTSettingKey.SpectatorsMaximum)) return true;

        var gameSlot = args.SenderGameUser.GameSlot;
        var userIdentifier = args.SenderGameUser.UserIdentifier;
        var connectionTag = args.SenderGameUser.GetGameConnectionTag();

        if (gameInfo.GameOwner == GameOwnerEnum.Server)
        {
            if (connectionTag == null) return true;
            if (connectionTag.GameUsers.Length > 1) return true;
        }

        gameSlot.ClearGameUser(null);

        args.SenderGameUser.GameSlot = null;
        args.SenderGameUser.JoinedAsSpectator = true;
        args.SenderGameUser.SpectatingWhileWaitingToPlay = false;

        Player senderGamePlayer = gameInfo.GameWorld?.GetPlayerByUserIdentifier(userIdentifier);
        senderGamePlayer?.SetUser(0);
        senderGamePlayer?.Kill();

        var messageKey = "menu.lobby.newPlayerJoinedTeam";
        var messageArgs = new string[] { args.SenderGameUser.GetProfileName(), LanguageHelper.GetText("general.spectator") };
        var messageColor = Color.LightGray;

        if (gameInfo.GameOwner == GameOwnerEnum.Server)
        {
            server.SendMessage(MessageType.ChatMessage, new NetMessage.ChatMessage.Data(messageKey, messageColor, messageArgs));
            server.SendMessage(MessageType.Sound, new NetMessage.Sound.Data("PlayerLeave", true, Vector2.Zero, 1f));
            server.SyncGameUserInfo(args.SenderGameUser);
            server.SyncGameSlotInfo(gameSlot);
        }
        else
        {
            gameInfo.ShowChatMessage(new(messageKey, messageColor, messageArgs));
            SoundHandler.PlayGlobalSound("PlayerLeave");
        }

        args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.spectate.message.info"), Color.Gray, args.SenderGameUser));
        return true;
    }

    internal static bool HandleCTHelp(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        var colYellow = Color.Yellow;
        var colOrange = new Color(255, 181, 26);
        var colLightGreen = new Color(159, 255, 64);

        var colOnlyHost = new Color(255, 91, 51);

        if (args.ModeratorPrivileges || !SFDCTConfig.Get<bool>(CTSettingKey.SpectatorsOnlyModerators))
        {
            args.Feedback.Add(new(args.SenderGameUser, "'/SPECTATE' become a spectator", colYellow, args.SenderGameUser));
            args.Feedback.Add(new(args.SenderGameUser, "'/JOIN' join back from spectating to an available game-slot", colYellow, args.SenderGameUser));
        }

        if (gameInfo.GameOwner == GameOwnerEnum.Server)
        {
            if (SFDCTConfig.Get<bool>(CTSettingKey.VoteKickEnabled))
            {
                args.Feedback.Add(new(args.SenderGameUser, "'/VOTEKICK' [PLAYER] to start a vote-kick against a player.", colYellow, args.SenderGameUser));
            }
        }

        if (!args.ModeratorPrivileges) return true;

        if (args.CanUseModeratorCommand("GRAVITY", "GRAV")) args.Feedback.Add(new(args.SenderGameUser, "'/GRAVITY [X] [Y]' to set the world's gravity.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("DAMAGE", "HURT")) args.Feedback.Add(new(args.SenderGameUser, "'/HURT [AMOUNT] [PLAYER]' to deal damage to a player, negative amounts heal.", colOrange, args.SenderGameUser));

        if (gameInfo.GameOwner == GameOwnerEnum.Server)
        {
            if (args.SenderGameUser.IsHost)
            {
                args.Feedback.Add(new(args.SenderGameUser, "'/MODCMD [A|R|L|C|T] [...]' to add/remove/list/clear/try moderator commands.", colOnlyHost, args.SenderGameUser));
            }

            if (args.CanUseModeratorCommand("M", "MOUSE", "DEBUGMOUSE")) args.Feedback.Add(new(args.SenderGameUser, "'/MOUSE' to enable or disable the debug mouse.", colLightGreen, args.SenderGameUser));
            if (args.CanUseModeratorCommand("SERVERMOVEMENT", "SVMOV")) args.Feedback.Add(new(args.SenderGameUser, "'/SERVERMOVEMENT [PLAYER] [0|1]' to control the server-movement state (empty is default, 0 is off, 1 is on).", colLightGreen, args.SenderGameUser));
            if (args.CanUseModeratorCommand("META")) args.Feedback.Add(new(args.SenderGameUser, "'/META [...]' to send a message with meta formatting.", colLightGreen, args.SenderGameUser));
            if (args.CanUseModeratorCommand("EXEC")) args.Feedback.Add(new(args.SenderGameUser, "'/EXEC [PATH/TO/FILE]' to execute a commands file.", colLightGreen, args.SenderGameUser));
        }

        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "Scroll the chat using the scroll-wheel to see all commands.", Color.LightBlue, args.SenderGameUser));
        return true;
    }
}
