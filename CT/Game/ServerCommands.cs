using Microsoft.Xna.Framework;
using SDR.Networking;
using SFD;
using SFD.Core;
using SFD.GUI.Text;
using SFD.Parser;
using SFD.Sounds;
using SFD.States;
using SFD.Voting;
using SFDCT.Configuration;
using SFDCT.Sync;
using SteamLayer.SteamManagers;
using Steamworks;
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

        string message = args.SourceParameters;

        server.SendMessage(MessageType.ChatMessage, new NetMessage.ChatMessage.Data(message, Color.White, args.SenderGameUser.GetProfileName(), true, args.SenderGameUser.UserIdentifier));

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

        if (shouldSave) SFDConfig.SaveConfig((SFDConfigSaveMode)10);

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
        ServerHandler.DebugMouse = !ServerHandler.DebugMouse;

        string message = LanguageHelper.GetText("sfdct.command.debugmouse.message", LanguageHelper.GetBooleanText(ServerHandler.DebugMouse));
        args.Feedback.Add(new(args.SenderGameUser, message));
        return true;
    }

    internal static bool HandleServerMovement(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        if (args.Parameters.Count == 0) return true;

        GameUser gameUser = gameInfo.GetGameUserByStringInput(args.Parameters[0], args.SenderGameUser);
        if (gameUser == null || gameUser.IsDisposed || gameUser.IsBot) return true;

        GameConnectionTag gameConnectionTag = gameUser.GetGameConnectionTag();
        if (gameConnectionTag == null || gameConnectionTag.IsDisposed || gameConnectionTag.GameUsers == null) return true;

        bool useServerMovement = !gameConnectionTag.ForceServerMovement;
        bool resetServerMovement = false;
        if (args.Parameters.Count >= 2)
        {
            if (args.Parameters[1].Equals("NULL", StringComparison.OrdinalIgnoreCase))
            {
                resetServerMovement = true;
            }
            else
            {
                bool.TryParse(args.Parameters[1], out useServerMovement);
            }
        }

        if (resetServerMovement)
        {
            gameConnectionTag.ForcedServerMovementToggleTime = Constants.HOST_GAME_FORCED_SERVER_MOVEMENT_TOGGLE_TIME_MS;
            gameConnectionTag.ForceServerMovement = false;
        }
        else
        {
            gameConnectionTag.ForcedServerMovementToggleTime = useServerMovement ? -1f : -2f;
            gameConnectionTag.ForceServerMovement = useServerMovement;
        }

        string message = LanguageHelper.GetText("sfdct.command.servermovement.message", gameUser.GetProfileName(), resetServerMovement ? "NULL" : LanguageHelper.GetBooleanText(useServerMovement));
        args.Feedback.Add(new(args.SenderGameUser, message, args.SenderGameUser));

        foreach (var connectionUser in gameConnectionTag.GameUsers)
        {
            connectionUser.ForceServerMovement = useServerMovement;

            Player playerByUserIdentifier = gameInfo.GameWorld.GetPlayerByUserIdentifier(connectionUser.UserIdentifier);
            playerByUserIdentifier?.UpdateCanDoPlayerAction();
        }

        return true;
    }

    internal static bool HandleVoteKick(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        if (!SFDCTConfig.Get<bool>(CTSettingKey.VoteKickEnabled)) return true;

        if (!Voting.GameVoteYesNo.CanStartVote(gameInfo))
        {
            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.fail"), Color.Red, args.SenderGameUser));
            return true;
        }

        if (args.Parameters.Count <= 0)
        {
            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.fail.nouser"), Color.Red, args.SenderGameUser));
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
        string userTokickAccountName = userToKick.IsBot ? "COM" : string.Empty;
        if (server.GameInfo.AccountNameInfo.TryGetAccountID(userToKick.UserIdentifier, out SteamId steamId))
        {
            userTokickAccountName = SteamIdNameManager.Instance.GetAccountName(steamId);
        }

        string kickOwnerUserProfileName = kickOwnerUser.GetProfileName();
        string kickOwnerUserAccountName = kickOwnerUser.IsBot ? "COM" : string.Empty;
        SteamId userToKickSteamId = 0L;
        if (server.GameInfo.AccountNameInfo.TryGetAccountID(kickOwnerUser.UserIdentifier, out userToKickSteamId))
        {
            kickOwnerUserAccountName = SteamIdNameManager.Instance.GetAccountName(userToKickSteamId);
        }

        if (!userToKickSteamId.IsValid)
        {
            return true;
        }

        string soundID = "PlayerLeave";

        long[] validRemoteUniqueIdentifiers = server.GetConnectedUniqueIdentifiers((NetConnection x) => x.GameConnectionTag() != null && x.GameConnectionTag().FirstGameUser != null && x.GameConnectionTag().FirstGameUser.UserIdentifier != userToKick.UserIdentifier && x.GameConnectionTag().FirstGameUser.CanVote);
        if (validRemoteUniqueIdentifiers.Length <= 3)
        {
            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.fail.notenoughusers"), Color.Red, args.SenderGameUser));
            return true;
        }

        ConsoleOutput.ShowMessage(ConsoleOutputType.Information, string.Format("Creating vote-kick from '{0}' ({1}) against '{2}' ({3})", kickOwnerUserProfileName, kickOwnerUserAccountName, userToKickProfileName, userTokickAccountName));

        var vote = new Voting.GameVoteKick(GameVote.GetNextVoteID(), userToKickProfileName, userTokickAccountName, userToKickSteamId);
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
        if (SFDCTConfig.Get<int>(CTSettingKey.SpectatorsMaximum) <= 0) return true;
        if (!args.SenderGameUser.IsModerator && SFDCTConfig.Get<bool>(CTSettingKey.SpectatorsOnlyModerators)) return true;

        int userCount;
        GameUser gameUser;
        List<GameSlot> gameSlots;

        if (gameInfo.GameOwner == GameOwnerEnum.Server)
        {
            GameConnectionTag connectionTag = args.SenderGameUser.GetGameConnectionTag();

            if (connectionTag == null) return true;
            if (connectionTag.GameUsers.Count() > 1)
            {
                args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.join.fail.localplayer"), Color.Red, args.SenderGameUser));
                return true;
            }

            if (connectionTag.FirstGameUser == null || connectionTag.FirstGameUser.IsDisposed) return true;
            if (!connectionTag.FirstGameUser.JoinedAsSpectator) return true;

            gameUser = connectionTag.FirstGameUser;
            userCount = connectionTag.GameUsers.Count();
            gameSlots = server.FindOpenGameSlots(gameInfo.DropInMode, userCount, gameInfo.EvenTeams, null);
        }
        else
        {
            gameUser = args.SenderGameUser;
            userCount = 1;
            gameSlots = [gameInfo.GameSlots[0]];
        }

        if (gameSlots != null && gameSlots.Count > 0)
        {
            GameSlot gameSlot = gameSlots.First();
            gameSlot.ClearGameUser(gameInfo);
            gameSlot.GameUser = gameUser;
            gameSlot.CurrentState = GameSlot.State.Occupied;
            gameUser.GameSlot = gameSlot;
            gameUser.JoinedAsSpectator = false;

            List<string> messArgs = [];
            string mess = "menu.lobby.newPlayerJoined";
            Color messColor = Constants.COLORS.PLAYER_CONNECTED;

            messArgs.Add(gameUser.GetProfileName());
            if (gameSlot.CurrentTeam != Team.Independent)
            {
                mess = "menu.lobby.newPlayerJoinedTeam";
            }

            if (gameInfo.GameOwner == GameOwnerEnum.Server)
            {
                server.SendMessage(MessageType.ChatMessageSuppressDSForm, new NetMessage.ChatMessage.Data(mess, messColor, messArgs.ToArray()));
                server.SendMessage(MessageType.Sound, new NetMessage.Sound.Data("PlayerJoin", true, Vector2.Zero, 1f), null);
                server.SyncGameSlotInfo(gameSlot);
                server.SyncGameUserInfo(gameUser, null);
            }
            else
            {
                gameInfo.ShowChatMessage(new(mess, messColor, messArgs.ToArray()));
                SoundHandler.PlayGlobalSound("PlayerJoin");
            }

            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.join.message"), Color.Gray, args.SenderGameUser));
        }
        else
        {
            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.join.fail.nogameslot"), Color.Red, args.SenderGameUser));
        }

        return true;
    }

    internal static bool HandleSpectatorSpectate(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        if (SFDCTConfig.Get<int>(CTSettingKey.SpectatorsMaximum) <= 0) return true;
        if (!args.SenderGameUser.IsModerator && SFDCTConfig.Get<bool>(CTSettingKey.SpectatorsOnlyModerators)) return true;
        if (gameInfo.GetSpectatingUsers().Count >= SFDCTConfig.Get<int>(CTSettingKey.SpectatorsMaximum)) return true;

        GameSlot gameSlot = null;
        int userIdentifier;
        GameConnectionTag connectionTag = null;

        if (gameInfo.GameOwner == GameOwnerEnum.Server)
        {
            connectionTag = args.SenderGameUser.GetGameConnectionTag();

            if (connectionTag == null) return true;
            if (connectionTag.GameUsers.Count() > 1)
            {
                args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.spectate.fail.localplayer"), Color.Red, args.SenderGameUser));
                return true;
            }

            if (connectionTag.FirstGameUser == null || connectionTag.FirstGameUser.IsDisposed) return true;
            if (connectionTag.FirstGameUser.JoinedAsSpectator) return true;

            gameSlot = connectionTag.FirstGameUser.GameSlot;
            userIdentifier = connectionTag.FirstGameUser.UserIdentifier;

            connectionTag.FirstGameUser.GameSlot = null;
            connectionTag.FirstGameUser.JoinedAsSpectator = true;
        }
        else
        {
            if (args.SenderGameUser.JoinedAsSpectator) return true;

            gameSlot = args.SenderGameUser.GameSlot;
            userIdentifier = args.SenderGameUserIdentifier;
        }

        gameSlot.GameUser = null;
        gameSlot.ClearGameUser(null);
        gameSlot.CurrentState = GameSlot.State.Open;

        Player senderGamePlayer = gameInfo.GameWorld?.GetPlayerByUserIdentifier(userIdentifier);
        senderGamePlayer?.SetUser(0);
        senderGamePlayer?.Kill();

        string mess = "menu.lobby.newPlayerJoinedTeam";
        string[] messArgs = [args.SenderGameUser.GetProfileName(), LanguageHelper.GetText("general.spectator")];
        Color messColor = Color.LightGray;

        if (gameInfo.GameOwner == GameOwnerEnum.Server)
        {
            server.SendMessage(MessageType.ChatMessageSuppressDSForm, new NetMessage.ChatMessage.Data(mess, messColor, messArgs), null, null);
            server.SendMessage(MessageType.Sound, new NetMessage.Sound.Data("PlayerLeave", true, Vector2.Zero, 1f), null);
            server.SyncGameSlotInfo(gameSlot, null);
            server.SyncGameUserInfo(connectionTag.FirstGameUser, null);
        }
        else
        {
            gameInfo.ShowChatMessage(new(mess, messColor, messArgs));
            SoundHandler.PlayGlobalSound("PlayerLeave");
        }

        args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.spectate.message.info"), Color.Gray, args.SenderGameUser));
        return true;
    }

    internal static bool HandleHelp(Server server, ProcessCommandArgs args, GameInfo gameInfo)
    {
        var colOrange = new Color(255, 181, 26);
        var colLightGreen = new Color(159, 255, 64);
        var colRedOrange = new Color(255, 91, 51);

        if (!args.SenderGameUser.IsModerator) return true;

        if (args.CanUseModeratorCommand("SLOMO", "SLOWMOTION")) args.Feedback.Add(new(args.SenderGameUser, "'/SLOMO [1/0]'", colLightGreen, args.SenderGameUser));
        if (args.CanUseModeratorCommand("SETTIME")) args.Feedback.Add(new(args.SenderGameUser, "'/SETTIME [0,1-2,0]'", colLightGreen, args.SenderGameUser));
        if (args.CanUseModeratorCommand("INFINITE_AMMO", "IA")) args.Feedback.Add(new(args.SenderGameUser, "'/INFINITE_AMMO [1/0]'", colLightGreen, args.SenderGameUser));
        if (args.CanUseModeratorCommand("INFINITE_LIFE", "INFINITE_HEALTH", "IL", "IH")) args.Feedback.Add(new(args.SenderGameUser, "'/INFINITE_LIFE [1/0]'", colLightGreen, args.SenderGameUser));
        if (args.CanUseModeratorCommand("INFINITE_ENERGY", "IE")) args.Feedback.Add(new(args.SenderGameUser, "'/INFINITE_ENERGY [1/0]'", colLightGreen, args.SenderGameUser));
        if (args.CanUseModeratorCommand("GIVE")) args.Feedback.Add(new(args.SenderGameUser, "'/GIVE [PLAYER] [ITEM]'", colLightGreen, args.SenderGameUser));
        if (args.CanUseModeratorCommand("REMOVE")) args.Feedback.Add(new(args.SenderGameUser, "'/REMOVE [PLAYER] [ITEM/SLOT]'", colLightGreen, args.SenderGameUser));

        args.Feedback.Add(new(args.SenderGameUser, "'/ITEMS' to list all available items in the game.", colLightGreen, args.SenderGameUser));

        if (args.CanUseModeratorCommand("SETSTARTHEALTH", "SETSTARTLIFE", "STARTHEALTH", "STARTLIFE")) args.Feedback.Add(new(args.SenderGameUser, "'/STARTLIFE [1-100]'", colLightGreen, args.SenderGameUser));
        if (args.CanUseModeratorCommand("STARTITEMS", "STARTITEM", "SETSTARTITEMS", "SETSTARTITEM", "SETSTARTUPITEMS", "SETSTARTUPITEM")) args.Feedback.Add(new(args.SenderGameUser, "'/SETSTARTITEMS ID ID ID ...' to set start items.", colLightGreen, args.SenderGameUser));
        if (args.CanUseModeratorCommand("CLEAR", "RESET")) args.Feedback.Add(new(args.SenderGameUser, "'/CLEAR' to reset cheats.", colLightGreen, args.SenderGameUser));

        if (args.SenderGameUser.IsHost && gameInfo.GameOwner != GameOwnerEnum.Local)
        {
            args.Feedback.Add(new(args.SenderGameUser, "'/CHAT [1/0]' to enable/disable global chat.", colRedOrange, args.SenderGameUser));
            args.Feedback.Add(new(args.SenderGameUser, "'/MODERATORS' to list all moderators with index.", colRedOrange, args.SenderGameUser));
            args.Feedback.Add(new(args.SenderGameUser, "'/ADDMODERATOR [PLAYER]' to add someone to the moderator list.", colRedOrange, args.SenderGameUser));
            args.Feedback.Add(new(args.SenderGameUser, "'/REMOVEMODERATOR [INDEX|PLAYER]' to remove from the moderator list.", colRedOrange, args.SenderGameUser));
            args.Feedback.Add(new(args.SenderGameUser, "'/SETMODPASS [INDEX|PLAYER] [PASS]' to set mod password.", colRedOrange, args.SenderGameUser));
        }

        if (args.CanUseModeratorCommand("MSG", "MESSAGE")) args.Feedback.Add(new(args.SenderGameUser, "'/MSG [TEXT]' to show a message to everyone.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("SERVERDESCRIPTION")) args.Feedback.Add(new(args.SenderGameUser, "'/SERVERDESCRIPTION' to show the server description as a reminder.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("GAMEOVER")) args.Feedback.Add(new(args.SenderGameUser, "'/GAMEOVER' to restart the game.", colOrange, args.SenderGameUser));

        if (GameSFD.Handle.CurrentState == State.EditorTestRun)
        {
            args.Feedback.Add(new(args.SenderGameUser, "'/RS' or '/RESTART' to restart instant.", colOrange, args.SenderGameUser));
        }

        if (args.CanUseModeratorCommand("SCRIPTS")) args.Feedback.Add(new(args.SenderGameUser, "'/SCRIPTS' to list all available scripts.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("LOADSCRIPT", "STARTSCRIPT")) args.Feedback.Add(new(args.SenderGameUser, "'/STARTSCRIPT X' to start script X.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("UNLOADSCRIPT", "STOPSCRIPT")) args.Feedback.Add(new(args.SenderGameUser, "'/STOPSCRIPT X' to stop script X.", colOrange, args.SenderGameUser));

        if (args.SenderGameUser.IsHost)
        {
            args.Feedback.Add(new(args.SenderGameUser, "'/RELOADSCRIPTS' to reload scripts from disk.", colOrange, args.SenderGameUser));
        }

        if (args.CanUseModeratorCommand("MAPS", "LISTMAPS", "SHOWMAPS")) args.Feedback.Add(new(args.SenderGameUser, "'/MAPS' to list all maps.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("MAPS", "LISTMAPS", "SHOWMAPS")) args.Feedback.Add(new(args.SenderGameUser, "'/MAPS [CATEGORY]' to list all maps in category X.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("LISTMAPCATEGORIES", "LISTMAPCAT", "SHOWMAPCATEGORIES", "SHOWMAPCAT", "LISTMC", "SHOWMC", "MAPCATEGORIES")) args.Feedback.Add(new(args.SenderGameUser, "'/MAPCATEGORIES' to list all map categories.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("CHANGEMAPCATEGORY", "CHANGEMAPCAT", "CHANGEMC")) args.Feedback.Add(new(args.SenderGameUser, "'/CHANGEMAPCATEGORY [CATEGORY]' to change the map category.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("MAP", "CHANGEMAP")) args.Feedback.Add(new(args.SenderGameUser, "'/CHANGEMAP [MAP]' to change the map next fight.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("NEXTMAP")) args.Feedback.Add(new(args.SenderGameUser, "'/NEXTMAP' to change map in the current map rotation to the next map.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("MAPPARTS", "SHOWMAPPARTS", "LISTMAPPARTS", "CHAPTERS", "LISTCHAPTERS")) args.Feedback.Add(new(args.SenderGameUser, "'/CHAPTERS' to list available chapters for the current map.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("SETMAPPART", "CHANGEMAPPART", "SMP", "CMP", "SETCHAPTER")) args.Feedback.Add(new(args.SenderGameUser, "'/SETCHAPTER [X]' to change to chapter X.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("NEXTMAPPART", "NEXTCHAPTER")) args.Feedback.Add(new(args.SenderGameUser, "'/NEXTCHAPTER' to change to the next chapter.", colOrange, args.SenderGameUser));

        if (args.CanUseModeratorCommand("MAPROTATION", "MR"))
        {
            args.Feedback.Add(new(args.SenderGameUser, "'/MAPROTATION [X]' to enable map rotation every X fights.", colOrange, args.SenderGameUser));
            args.Feedback.Add(new(args.SenderGameUser, "'/MAPROTATION [M]' to change map rotation mode where M is A, B, C or D.", colOrange, args.SenderGameUser));
            args.Feedback.Add(new(args.SenderGameUser, "'/MAPROTATION [M] [X]' to change map rotation mode and interval.", colOrange, args.SenderGameUser));
        }

        if (args.CanUseModeratorCommand("SETDIFFICULTY"))
        {
            args.Feedback.Add(new(args.SenderGameUser, "'/SETDIFFICULTY [1/2/3/4/EASY/NORMAL/HARD/EXPERT]' to change the difficulty for campaign maps.", colOrange, args.SenderGameUser));
        }

        if (gameInfo.GameOwner != GameOwnerEnum.Local)
        {
            if (args.CanUseModeratorCommand("BAN", "BAN_USER")) args.Feedback.Add(new(args.SenderGameUser, "'/BAN [PLAYER]' to ban a player by name or index.", colOrange, args.SenderGameUser));

            if (args.CanUseModeratorCommand("KICK", "KICK_USER"))
            {
                args.Feedback.Add(new(args.SenderGameUser, "'/KICK [PLAYER]' to kick player by name or index.", colOrange, args.SenderGameUser));
                args.Feedback.Add(new(args.SenderGameUser, "'/KICK [X] [PLAYER]' to kick a player by name or index for X minutes (max 60 minutes).", colOrange, args.SenderGameUser));
            }

            if (args.CanUseModeratorCommand("MAXPING", "MAX_PING")) args.Feedback.Add(new(args.SenderGameUser, "'/MAXPING [X]' to set a maximum ping to X (range 50-500). 0 to disable.", colOrange, args.SenderGameUser));
            if (args.CanUseModeratorCommand("AUTO_KICK_AFK", "AUTOKICKAFK", "KICK_AFK", "KICKAFK", "AUTO_KICK_IDLE", "AUTOKICKIDLE", "KICK_IDLE", "KICKIDLE")) args.Feedback.Add(new(args.SenderGameUser, "'/KICKIDLE [X]' to set a maximum idle time to X seconds (range 30-600). 0 to disable.", colOrange, args.SenderGameUser));
        }

        if (args.CanUseModeratorCommand("TIMELIMIT", "TL")) args.Feedback.Add(new(args.SenderGameUser, "'/TIMELIMIT [X]' to set time limit to X seconds (range 30-600). 0=disable.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("SUDDENDEATH", "SD")) args.Feedback.Add(new(args.SenderGameUser, "'/SUDDENDEATH [1/0]' to set sudden death on/off.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("SHUFFLETEAMS", "ST")) args.Feedback.Add(new(args.SenderGameUser, "'/SHUFFLETEAMS' to shuffle the teams next fight.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("SHUFFLETEAMS", "ST")) args.Feedback.Add(new(args.SenderGameUser, "'/SHUFFLETEAMS [X]' to shuffle the teams each X fights.", colOrange, args.SenderGameUser));
        if (args.CanUseModeratorCommand("SETTEAMS")) args.Feedback.Add(new(args.SenderGameUser, "'/SETTEAMS 00000000' to set new teams next fight. 0=independent, 1=team1...", colOrange, args.SenderGameUser));

        args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, "Scroll the chat using the scroll-wheel to see all commands.", Color.LightBlue, args.SenderGameUser));
        return true;
    }
}
