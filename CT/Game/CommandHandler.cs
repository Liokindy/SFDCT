using HarmonyLib;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using SFD;
using SFD.Core;
using SFD.GUI.Text;
using SFD.Parser;
using SFD.Sounds;
using SFD.Voting;
using SFDCT.Configuration;
using SFDCT.Misc;
using SFDCT.Sync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class CommandHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.HandleCommand), typeof(ProcessCommandArgs))]
    private static bool GameInfo_HandleCommand_Prefix_CustomCommands(ref bool __result, ProcessCommandArgs args, GameInfo __instance)
    {
        bool ranCustomCommand = false;

        if (__instance.GameOwner == GameOwnerEnum.Client || __instance.GameOwner == GameOwnerEnum.Local)
        {
            ranCustomCommand = ClientCommands(args, __instance);
        }

        if (!ranCustomCommand && (__instance.GameOwner == GameOwnerEnum.Server || __instance.GameOwner == GameOwnerEnum.Local))
        {
            ranCustomCommand = ServerCommands(args, __instance);
        }

        __result = ranCustomCommand;
        return !ranCustomCommand;
    }

    internal static bool ClientCommands(ProcessCommandArgs args, GameInfo gameInfo)
    {
        Client client = GameSFD.Handle.Client;
        if (client == null && gameInfo.GameOwner == GameOwnerEnum.Client)
        {
            return false;
        }

        if (args.IsCommand("PLAYERS", "LISTPLAYERS", "SHOWPLAYERS", "USERS", "LISTUSERS", "SHOWUSERS"))
        {
            string user = LanguageHelper.GetText("sfdct.command.players.message.user");
            string bot = LanguageHelper.GetText("sfdct.command.players.message.bot");

            int gameUserCount = 0;
            using (IEnumerator<GameUser> enumerator = gameInfo.GetGameUsers().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    GameUser gameUser = enumerator.Current;
                    string mess = user;
                    string status = "";
                    Color messColor = Color.LightBlue;

                    if (gameUser.IsModerator) { status = "MODERATOR"; messColor = Color.LightGreen; }
                    if (gameUser.IsHost) { status = "HOST"; messColor = Color.LightPink; }
                    if (gameUser.SpectatingWhileWaitingToPlay) { status = "WAITING"; }
                    if (gameUser.JoinedAsSpectator) { status = "SPECTATOR"; }
                    if (gameUser.IsBot) { status = "BOT"; mess = bot; }

                    messColor = new Color((Color.LightBlue.R + messColor.R) / 2, (Color.LightBlue.G + messColor.G) / 2, (Color.LightBlue.B + messColor.B) / 2);
                    messColor *= gameUserCount % 2 == 0 ? 0.8f : 0.9f;
                    args.Feedback.Add(new(args.SenderGameUser, string.Format(mess, gameUser.GameSlotIndex, gameUser.GetProfileName(), gameUser.AccountName, status), messColor, args.SenderGameUser));

                    gameUserCount++;
                }
            }

            string footer = LanguageHelper.GetText("sfdct.command.players.footer", gameUserCount.ToString());
            args.Feedback.Add(new(args.SenderGameUser, footer, Color.LightBlue, args.SenderGameUser));

            return true;
        }

        if (args.IsCommand("CLEARCHAT"))
        {
            GameChat.ClearChat();
            return true;
        }

        return false;
    }

    internal static bool IsAndCanUseModeratorCommand(ProcessCommandArgs args, params string[] commands)
    {
        if (!args.IsCommand(commands)) return false;

        return args.CanUseModeratorCommand(commands);
    }

    internal static ProcessCommandArgs ExecuteCommandsFile(ProcessCommandArgs args, GameInfo gameInfo, string fileName)
    {
        fileName = fileName.Trim();
        fileName = fileName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        try
        {
            string filePath = Path.Combine(Globals.Paths.Commands, fileName);
            filePath = Path.GetFullPath(filePath);
            filePath = Path.ChangeExtension(filePath, ".txt");

            if (!File.Exists(filePath))
            {
                args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.exec.fail.nofile"), Color.Red, args.SenderGameUser));
                return args;
            }

            string[] fileLines = File.ReadAllLines(filePath);

            foreach (string line in fileLines)
            {
                string command = line.Trim();

                if (string.IsNullOrWhiteSpace(command)) continue;
                if (string.IsNullOrEmpty(command)) continue;
                if (command.StartsWith("//")) continue;
                if (!command.StartsWith("/")) command = "/" + command;
                if (command.StartsWith("/exec", StringComparison.OrdinalIgnoreCase) && command.EndsWith(fileName)) continue;

                // re-use the same args object to show feedback messages
                args.SourceCommand = command.Remove(0, 1).Trim();
                args.CommandValue = command;
                args.SourceParameters = "";
                args.Parameters.Clear();
                args.Feedback.Clear();

                int spaceIndex = args.SourceCommand.IndexOf(' ');
                if (spaceIndex > 0)
                {
                    args.CommandValue = args.SourceCommand.Substring(0, spaceIndex);
                    args.SourceParameters = args.SourceCommand.Substring(spaceIndex + 1);
                    args.Parameters.AddRange(args.SourceParameters.Split([' '], StringSplitOptions.RemoveEmptyEntries));
                }

                args.CommandValue = args.CommandValue.ToUpperInvariant();

                if (!gameInfo.HandleCommand(args))
                {
                    gameInfo.HandleMessageInScripts(args.SenderGameUserIdentifier, command);
                }

                gameInfo.ShowCommandFeedback(args);
            }

            args.Feedback.Clear();
        }
        catch (Exception ex)
        {
            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.exec.fail.error"), Color.Red, args.SenderGameUser));

            ConsoleOutput.ShowMessage(ConsoleOutputType.Error, string.Format("Exception trying to execute commands file: '{0}'", fileName));
            ConsoleOutput.ShowMessage(ConsoleOutputType.Error, ex.Message);
        }

        return args;
    }

    internal static bool ServerCommands(ProcessCommandArgs args, GameInfo gameInfo)
    {
        Server server = GameSFD.Handle.Server;
        if (server == null && gameInfo.GameOwner == GameOwnerEnum.Server) return false;

        if (args.HostPrivileges)
        {
            if (args.IsCommand("MOUSEMODERATORS", "MOUSEMOD"))
            {
                ServerHandler.DebugMouseOnlyHost = !ServerHandler.DebugMouseOnlyHost;

                string message = LanguageHelper.GetText("sfdct.command.debugmousemoderators.message", LanguageHelper.GetBooleanText(ServerHandler.DebugMouseOnlyHost));
                args.Feedback.Add(new(args.SenderGameUser, message));
                return true;
            }

            if (args.IsCommand("MODCMD", "MODCMDS", "MODCOMMANDS", "MODCOMMAND"))
            {
                string action = "HELP";
                List<string> commands = [];

                if (args.Parameters.Count >= 1) action = args.Parameters[0].ToUpperInvariant();
                if (args.Parameters.Count >= 2) commands = args.Parameters.GetRange(1, args.Parameters.Count - 1);
                if (args.Parameters.Count == 2 && args.Parameters[1] == "*") commands = GameInfo.ALL_MODERATOR_COMMANDS.ToList();

                string header1, header2, message1;
                Color moderatorAccentColor = new(159, 255, 64);
                bool shouldSave = false;

                switch (action)
                {
                    default:
                    case "HELP":
                        header1 = LanguageHelper.GetText("sfdct.command.modcommands.header.help");

                        args.Feedback.Add(new(args.SenderGameUser, header1, moderatorAccentColor, args.SenderGameUser));
                        break;
                    case "L":
                    case "LIST":
                        header1 = LanguageHelper.GetText("sfdct.command.modcommands.header.list");
                        message1 = LanguageHelper.GetText("sfdct.generic.list");

                        args.Feedback.Add(new(args.SenderGameUser, header1, moderatorAccentColor, args.SenderGameUser));
                        args.Feedback.Add(new(args.SenderGameUser, string.Format(message1, string.Join(" ", Constants.MODDERATOR_COMMANDS)), moderatorAccentColor * 0.5f, args.SenderGameUser));
                        break;
                    case "C":
                    case "CLEAR":
                        header1 = LanguageHelper.GetText("sfdct.command.modcommands.header.clear");

                        int clearedCount = 0;
                        shouldSave = true;

                        clearedCount += Constants.MODDERATOR_COMMANDS.Count;
                        Constants.MODDERATOR_COMMANDS.Clear();

                        args.Feedback.Add(new(args.SenderGameUser, string.Format(header1, clearedCount), moderatorAccentColor, args.SenderGameUser));
                        break;
                    case "R":
                    case "REMOVE":
                        if (commands.Count > 0)
                        {
                            header1 = LanguageHelper.GetText("sfdct.command.modcommands.header.remove");
                            message1 = LanguageHelper.GetText("sfdct.generic.list");

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

                            args.Feedback.Add(new(args.SenderGameUser, string.Format(header1, removedCount), moderatorAccentColor, args.SenderGameUser));
                            if (removedCount > 0)
                            {
                                args.Feedback.Add(new(args.SenderGameUser, string.Format(message1, string.Join(" ", commands)), moderatorAccentColor * 0.5f, args.SenderGameUser));
                            }
                        }
                        break;
                    case "A":
                    case "ADD":
                        if (commands.Count > 0)
                        {
                            header1 = LanguageHelper.GetText("sfdct.command.modcommands.header.add");
                            message1 = LanguageHelper.GetText("sfdct.generic.list");

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

                            args.Feedback.Add(new(args.SenderGameUser, string.Format(header1, addedCount), moderatorAccentColor, args.SenderGameUser));
                            if (addedCount > 0)
                            {
                                args.Feedback.Add(new(args.SenderGameUser, string.Format(message1, string.Join(" ", commands)), moderatorAccentColor * 0.5f, args.SenderGameUser));
                            }
                        }
                        break;
                    case "T":
                    case "TRY":
                        if (commands.Count > 0)
                        {
                            header1 = LanguageHelper.GetText("sfdct.command.modcommands.header.try.true");
                            header2 = LanguageHelper.GetText("sfdct.command.modcommands.header.try.false");
                            message1 = LanguageHelper.GetText("sfdct.generic.list");

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

                            args.Feedback.Add(new(args.SenderGameUser, string.Format(header2), moderatorAccentColor, args.SenderGameUser));
                            if (canNotUseList.Count > 0)
                            {
                                args.Feedback.Add(new(args.SenderGameUser, string.Format(message1, string.Join(" ", canNotUseList)), moderatorAccentColor * 0.5f, args.SenderGameUser));
                            }
                            args.Feedback.Add(new(args.SenderGameUser, string.Format(header1), moderatorAccentColor, args.SenderGameUser));

                            if (canUseList.Count > 0)
                            {
                                args.Feedback.Add(new(args.SenderGameUser, string.Format(message1, string.Join(" ", canUseList)), moderatorAccentColor * 0.5f, args.SenderGameUser));
                            }
                        }
                        break;
                }

                if (shouldSave) SFDConfig.SaveConfig((SFDConfigSaveMode)10);

                return true;
            }

            if (args.IsCommand("META"))
            {
                if (string.IsNullOrEmpty(args.SourceParameters))
                {
                    string exampleMetaText = "- Default. [#FF00FF]Magenta[#]. [#FF0]Yellow[#]. Icon [ICO=TEAM_1]";

                    args.Feedback.Add(new(args.SenderGameUser, "Meta-formatting allows to specify text color ('[#FFFFFF]'), reset text color ('[#]'), and display icons ('[ICO=]'). Example:", args.SenderGameUser));
                    args.Feedback.Add(new(args.SenderGameUser, exampleMetaText, true, Constants.COLORS.LIGHT_GRAY, args.SenderGameUser));
                    args.Feedback.Add(new(args.SenderGameUser, exampleMetaText, false, Constants.COLORS.LIGHT_GRAY, args.SenderGameUser));

                    string availableIconsText = "- ";
                    foreach (string iconKey in TextIcons.m_icons.Keys)
                    {
                        availableIconsText = availableIconsText + string.Format("{1} ([ICO={0}])", iconKey, TextMeta.EscapeText(iconKey)) + ", ";
                    }
                    availableIconsText.Remove(availableIconsText.Length - 2); // remove last ", "

                    args.Feedback.Add(new(args.SenderGameUser, "Available Icons:", args.SenderGameUser));
                    args.Feedback.Add(new(args.SenderGameUser, availableIconsText, true, Constants.COLORS.LIGHT_GRAY, args.SenderGameUser));
                }
                else
                {
                    string message = args.SourceParameters;

                    if (args.Origin == HandleCommandOrigin.User)
                    {
                        message = string.Format("[ICO=TEAM_{0}][{1}]{2}:[#] [{3}]{4}",
                                        (int)args.SenderGameUser.TeamIcon,
                                        Constants.COLORS.GetTeamColor(args.SenderGameUser.TeamIcon, Constants.COLORS.TeamColorType.ChatName).ToHex(),
                                        TextMeta.EscapeText(args.SenderGameUser.GetProfileName()),
                                        Constants.COLORS.CHAT_ALL_MESSAGE.ToHex(),
                                        args.SourceParameters
                        );
                    }

                    server.SendMessage(MessageType.ChatMessage, new NetMessage.ChatMessage.Data(message, Color.White, args.SenderGameUser.GetProfileName(), true, args.SenderGameUser.UserIdentifier));
                }

                return true;
            }
        }

        if (args.ModeratorPrivileges)
        {
            if (gameInfo.GameWorld != null)
            {
                if (IsAndCanUseModeratorCommand(args, "GRAVITY", "GRAV"))
                {
                    if (args.Parameters.Count <= 0) return true;

                    float gx = 0f;
                    float gy = -26f;

                    if (args.Parameters.Count == 1)
                    {
                        if (!SFDXParser.TryParseFloat(args.Parameters[0], out gy))
                        {
                            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.gravity.fail.parsey"), args.SenderGameUser));
                            return true;
                        }
                    }

                    if (args.Parameters.Count == 2)
                    {
                        if (!SFDXParser.TryParseFloat(args.Parameters[0], out gx))
                        {
                            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.gravity.fail.parsex"), args.SenderGameUser));
                            return true;
                        }

                        if (!SFDXParser.TryParseFloat(args.Parameters[1], out gy))
                        {
                            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.gravity.fail.parsey"), args.SenderGameUser));
                            return true;
                        }
                    }

                    Vector2 gravityVector = new Vector2(gx, gy);
                    string message = LanguageHelper.GetText("sfdct.command.gravity.message", gravityVector.ToString());

                    gameInfo.GameWorld.GetActiveWorld.Gravity = gravityVector;
                    gameInfo.GameWorld.GetBackgroundWorld.Gravity = gravityVector;

                    args.Feedback.Add(new(args.SenderGameUser, message));
                    return true;
                }

                if (IsAndCanUseModeratorCommand(args, "DAMAGE", "HURT"))
                {
                    if (args.Parameters.Count <= 1) return true;

                    float damage = 0f;
                    if (!SFDXParser.TryParseFloat(args.Parameters[1], out damage)) return true;

                    GameUser user = gameInfo.GetGameUserByStringInput(args.Parameters[0], args.SenderGameUser);
                    if (user == null || user.IsDisposed) return true;

                    Player userPlayer = gameInfo.GameWorld.GetPlayerByUserIdentifier(user.UserIdentifier);
                    if (userPlayer == null || userPlayer.IsDisposed) return true;

                    if (damage >= 0f)
                    {
                        userPlayer.TakeMiscDamage(damage, false);
                    }
                    else
                    {
                        userPlayer.HealAmount(-damage);
                    }

                    string message = LanguageHelper.GetText("sfdct.command.damage.message", damage.ToString(), user.GetProfileName());
                    args.Feedback.Add(new(args.SenderGameUser, message));
                    return true;
                }
            }

            if (IsAndCanUseModeratorCommand(args, "M", "MOUSE", "DEBUGMOUSE"))
            {
                ServerHandler.DebugMouse = !ServerHandler.DebugMouse;

                string message = LanguageHelper.GetText("sfdct.command.debugmouse.message", LanguageHelper.GetBooleanText(ServerHandler.DebugMouse));
                args.Feedback.Add(new(args.SenderGameUser, message));
                return true;
            }

            if (IsAndCanUseModeratorCommand(args, "EXEC"))
            {
                args = ExecuteCommandsFile(args, gameInfo, args.SourceParameters);
                return true;
            }

            if (gameInfo.GameOwner == GameOwnerEnum.Server)
            {
                if (IsAndCanUseModeratorCommand(args, "SERVERMOVEMENT", "SVMOV"))
                {
                    if (args.Parameters.Count <= 0) return true;

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

                    string message = LanguageHelper.GetText("sfdct.command.servermovement.message", gameUser.AccountName, resetServerMovement ? "NULL" : LanguageHelper.GetBooleanText(useServerMovement));
                    args.Feedback.Add(new(args.SenderGameUser, message, args.SenderGameUser));

                    foreach (var connectionUser in gameConnectionTag.GameUsers)
                    {
                        connectionUser.ForceServerMovement = useServerMovement;

                        Player playerByUserIdentifier = gameInfo.GameWorld.GetPlayerByUserIdentifier(connectionUser.UserIdentifier);
                        playerByUserIdentifier?.UpdateCanDoPlayerAction();
                    }
                }
            }
        }

        if (gameInfo.GameOwner == GameOwnerEnum.Server)
        {
            if (args.IsCommand("VOTEKICK"))
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

                Voting.GameVoteKick vote = new(GameVote.GetNextVoteID(), userToKick);
                vote.ValidRemoteUniqueIdentifiers.AddRange(validRemoteUniqueIdentifiers);

                server.SendMessage(MessageType.GameVote, new Pair<GameVote, bool>(vote, false));
                server.SendMessage(MessageType.Sound, new NetMessage.Sound.Data(soundID, true, Vector2.Zero, 1f));

                gameInfo.VoteInfo.AddVote(vote);
                args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.message", kickOwnerUserProfileName, kickOwnerUserAccountName, userToKickProfileName, userTokickAccountName), Color.Yellow));
                args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.message.victim", kickOwnerUserProfileName, kickOwnerUserAccountName), Color.Yellow * 0.6f, userToKick));
                args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.message.owner", userToKickProfileName, userTokickAccountName), Color.Yellow * 0.6f, args.SenderGameUser));
                return true;
            }
        }

        if (args.IsCommand("JOIN"))
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

        if (args.IsCommand("SPECTATE"))
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
                    args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.spectate.fail.localplayer"), Color.Red, args.SenderGameUser, null));
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

        return false;
    }
}