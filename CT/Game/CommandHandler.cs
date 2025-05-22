using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Lidgren.Network;
using SFDCT.Sync;
using SFDCT.Configuration;
using SFD;
using SFD.Voting;
using SFD.Core;
using HarmonyLib;

namespace SFDCT.Game;

[HarmonyPatch]
internal static class CommandHandler
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameInfo), nameof(GameInfo.HandleCommand), typeof(ProcessCommandArgs))]
    private static bool GameInfo_HandleCommands(ref bool __result, ProcessCommandArgs args, GameInfo __instance)
    {
        bool ranCustomCommand = false;

        // We check to see if we handle custom commands before vanilla ones,
        // this also allows us to replace them.
        if (__instance.GameOwner == GameOwnerEnum.Client || __instance.GameOwner == GameOwnerEnum.Local)
        {
            // Client
            ranCustomCommand = ClientCommands(args, __instance);
        }
        if (!ranCustomCommand && (__instance.GameOwner == GameOwnerEnum.Server || __instance.GameOwner == GameOwnerEnum.Local))
        {
            // Server
            ranCustomCommand = ServerCommands(args, __instance);
        }

        // Don't execute the original method if a custom command was run.
        __result = ranCustomCommand;
        return !ranCustomCommand;
    }

    private static bool ClientCommands(ProcessCommandArgs args, GameInfo __instance)
    {
        Client client = GameSFD.Handle.Client;
        if (client == null && __instance.GameOwner == GameOwnerEnum.Client)
        {
            return false;
        }

        if (args.IsCommand("PLAYERS", "LISTPLAYERS", "SHOWPLAYERS", "USERS", "LISTUSERS", "SHOWUSERS"))
        {
            string user = LanguageHelper.GetText("sfdct.command.players.message.user");
            string bot = LanguageHelper.GetText("sfdct.command.players.message.bot");

            int gameUserCount = 0;
            using (IEnumerator<GameUser> enumerator = __instance.GetGameUsers().GetEnumerator())
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

        // Client-only commands (no offline)
        if (__instance.GameOwner == GameOwnerEnum.Client)
        {
            if (args.IsCommand("CLIENTMOUSE"))
            {
                WorldHandler.ClientMouse = !WorldHandler.ClientMouse;

                string message = LanguageHelper.GetText("sfdct.command.clientmouse.message", WorldHandler.ClientMouse.ToString());
                args.Feedback.Add(new(args.SenderGameUser, message, Color.LightBlue, args.SenderGameUser));
            }
        }

        return false;
    }

    private static bool ServerCommands(ProcessCommandArgs args, GameInfo __instance)
    {
        Server server = GameSFD.Handle.Server;
        if (server == null && __instance.GameOwner == GameOwnerEnum.Server)
        {
            return false;
        }

        // Host commands
        if (args.HostPrivileges)
        {
            if (args.IsCommand("SERVERMOUSEMODERATORS", "SERVERMOUSEMOD"))
            {
                WorldHandler.ServerMouseNoModerators = !WorldHandler.ServerMouseNoModerators;

                string message = LanguageHelper.GetText("sfdct.command.servermousemoderators.message", WorldHandler.ServerMouseNoModerators.ToString());
                args.Feedback.Add(new(args.SenderGameUser, message));
            }

            Color c1 = new Color(159, 255, 64);
            if (args.IsCommand("ADDMODCOMMANDS"))
            {
                string header = LanguageHelper.GetText("sfdct.command.addmodcommands.header");
                string message = LanguageHelper.GetText("sfdct.command.addmodcommands.message");
                string footer = LanguageHelper.GetText("sfdct.command.addmodcommands.footer");

                args.Feedback.Add(new(args.SenderGameUser, header, c1, args.SenderGameUser));
                for (int i = 0; i < args.Parameters.Count; i++)
                {
                    string modCommand = args.Parameters[i].ToUpperInvariant();

                    args.Feedback.Add(new(args.SenderGameUser, string.Format(message, modCommand), c1 * 0.5f, args.SenderGameUser));
                    Constants.MODDERATOR_COMMANDS.Add(modCommand);
                }
                args.Feedback.Add(new(args.SenderGameUser, string.Format(footer, args.Parameters.Count), c1 * 0.75f, args.SenderGameUser));

                SFDConfig.SaveConfig(SFDConfigSaveMode.HostGameOptions);
                return true;
            }

            if (args.IsCommand("REMOVEMODCOMMANDS") && args.Parameters.Count > 0)
            {
                string header = LanguageHelper.GetText("sfdct.command.removemodcommands.header");
                string message = LanguageHelper.GetText("sfdct.command.removemodcommands.message");
                string footer = LanguageHelper.GetText("sfdct.command.removemodcommands.footer");

                args.Feedback.Add(new(args.SenderGameUser, header, c1, args.SenderGameUser));
                for (int i = 0; i < args.Parameters.Count; i++)
                {
                    string modCommand = args.Parameters[i];

                    if (Constants.MODDERATOR_COMMANDS.Contains(modCommand.ToUpperInvariant()))
                    {
                        Constants.MODDERATOR_COMMANDS.Remove(modCommand.ToUpperInvariant());
                        args.Feedback.Add(new(args.SenderGameUser, string.Format(message, modCommand.ToUpperInvariant()), c1 * 0.5f, args.SenderGameUser));
                    }

                    Constants.MODDERATOR_COMMANDS.Add(modCommand);
                }
                args.Feedback.Add(new(args.SenderGameUser, string.Format(footer, args.Parameters.Count), c1 * 0.75f, args.SenderGameUser));

                SFDConfig.SaveConfig(SFDConfigSaveMode.HostGameOptions);
                return true;
            }

            if (args.IsCommand("CLEARMODCOMMANDS"))
            {
                string header = LanguageHelper.GetText("sfdct.command.clearmodcommands.header");
                string footer = LanguageHelper.GetText("sfdct.command.clearmodcommands.footer");

                args.Feedback.Add(new(args.SenderGameUser, header, c1, args.SenderGameUser));
                int count = Constants.MODDERATOR_COMMANDS.Count;
                Constants.MODDERATOR_COMMANDS.Clear();

                args.Feedback.Add(new(args.SenderGameUser, string.Format(footer, count), c1 * 0.75f, args.SenderGameUser));

                SFDConfig.SaveConfig(SFDConfigSaveMode.HostGameOptions);
                return true;
            }

            if (args.IsCommand("LISTMODCOMMANDS"))
            {
                string header = LanguageHelper.GetText("sfdct.command.listmodcommands.header");
                string message = LanguageHelper.GetText("sfdct.command.listmodcommands.message");

                args.Feedback.Add(new(args.SenderGameUser, header, c1, args.SenderGameUser));

                if (Constants.MODDERATOR_COMMANDS.Count > 0)
                {
                    for (int i = 0; i < Constants.MODDERATOR_COMMANDS.Count; i++)
                    {
                        string modCommand = Constants.MODDERATOR_COMMANDS[i];
                        args.Feedback.Add(new(args.SenderGameUser, string.Format(modCommand, modCommand), c1 * 0.5f, args.SenderGameUser));
                    }
                    
                    string footer = LanguageHelper.GetText("sfdct.command.listmodcommands.footer");
                    args.Feedback.Add(new(args.SenderGameUser, string.Format(footer, Constants.MODDERATOR_COMMANDS.Count), c1 * 0.8f, args.SenderGameUser));
                }
                else
                {
                    string footer = LanguageHelper.GetText("sfdct.command.listmodcommands.footer.all");
                    args.Feedback.Add(new(args.SenderGameUser, footer, c1 * 0.75f, args.SenderGameUser));
                }
                return true;
            }

            // Server-only commands (no offline)
            if (__instance.GameOwner == GameOwnerEnum.Server)
            {
                // Enables debug functions of the editor, i.e
                // Mouse-dragging, mouse-deletion, etc.
                if (args.IsCommand("MOUSE", "SERVERMOUSE"))
                {
                    WorldHandler.ServerMouse = !WorldHandler.ServerMouse;

                    string message = LanguageHelper.GetText("sfdct.command.servermouse.message", WorldHandler.ServerMouse.ToString());
                    args.Feedback.Add(new(args.SenderGameUser, message, null, null));

                    EditorDebugFlagSignalData signalData = new() { Enabled = WorldHandler.ServerMouse };
                    server.SendMessage(MessageType.Signal, new NetMessage.Signal.Data((NetMessage.Signal.Type)30, signalData.Store()));
                    return true;
                }
            }
        }

        // Moderator commands
        // -    This can only be true if Constants.MODDERATOR_COMMANDS is empty,
        //      contains the command or the user is the host checking
        //      CanUseModeratorCommand is useless
        if (args.ModeratorPrivileges)
        {
            // Commands that interact with the gameworld
            if (__instance.GameWorld != null)
            {
                if (args.IsCommand("GRAVITY", "GRAV"))
                {
                    if (args.Parameters.Count <= 0) return true;

                    // Default
                    float gx = 0f;
                    float gy = -26f;

                    if (args.Parameters.Count == 1)
                    {
                        if (!float.TryParse(args.Parameters[0].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out gy))
                        {
                            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.gravity.fail.parsey"), args.SenderGameUser));
                            return true;
                        }
                    }

                    if (args.Parameters.Count == 2)
                    {
                        if (!float.TryParse(args.Parameters[0].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out gx))
                        {
                            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.gravity.fail.parsex"), args.SenderGameUser));
                            return true;
                        }

                        if (!float.TryParse(args.Parameters[1].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out gy))
                        {
                            args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.gravity.fail.parsey"), args.SenderGameUser));
                            return true;
                        }
                    }

                    Vector2 gravityVector = new Vector2(gx, gy);
                    string message = LanguageHelper.GetText("sfdct.command.gravity.message", gravityVector.ToString());

                    __instance.GameWorld.GetActiveWorld.Gravity = gravityVector;
                    __instance.GameWorld.GetBackgroundWorld.Gravity = gravityVector;

                    args.Feedback.Add(new(args.SenderGameUser, message));
                    return true;
                }

                if (args.IsCommand("DAMAGE", "HURT"))
                {
                    if (args.Parameters.Count <= 1)
                    {
                        return true;
                    }

                    float damage = 0f;
                    if (!float.TryParse(args.Parameters[1].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out damage))
                    {
                        return true;
                    }

                    GameUser user = __instance.GetGameUserByStringInput(args.Parameters[0], args.SenderGameUser);
                    if (user != null && !user.IsDisposed)
                    {
                        Player userPlayer = __instance.GameWorld.GetPlayerByUserIdentifier(user.UserIdentifier);
                        if (userPlayer != null && !userPlayer.IsDisposed)
                        {
                            userPlayer.TakeMiscDamage(damage, false);

                            string message = LanguageHelper.GetText("sfdct.command.damage.message", damage.ToString(), user.GetProfileName());
                            args.Feedback.Add(new(args.SenderGameUser, message));
                            return true;
                        }
                    }

                    return true;
                }
            }

            // Server-only commands (no offline)
            if (__instance.GameOwner == GameOwnerEnum.Server)
            {
                if (args.IsCommand("SERVERMOVEMENT", "SVMOV"))
                {
                    if (args.Parameters.Count <= 0) return true;

                    GameUser gameUser = __instance.GetGameUserByStringInput(args.Parameters[0], args.SenderGameUser);
                    if (gameUser != null && !gameUser.IsDisposed)
                    {
                        GameConnectionTag gameConnectionTag = gameUser.GetGameConnectionTag();
                        if (gameConnectionTag != null && !gameConnectionTag.IsDisposed)
                        {
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

                            if (gameConnectionTag.GameUsers != null)
                            {
                                foreach (var connectionUser in gameConnectionTag.GameUsers)
                                {
                                    connectionUser.ForceServerMovement = useServerMovement;

                                    Player playerByUserIdentifier = __instance.GameWorld.GetPlayerByUserIdentifier(connectionUser.UserIdentifier);
                                    playerByUserIdentifier?.UpdateCanDoPlayerAction();
                                }
                            }
                        }
                    }
                }
            }
        }

        // Public commands
        if (__instance.GameOwner == GameOwnerEnum.Server)
        {
            if (args.IsCommand("VOTEKICK"))
            {
                if (args.Parameters.Count <= 0)
                {
                    args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.fail.nouser"), Color.Red, args.SenderGameUser));
                    return true;
                }

                if (__instance.VoteInfo == null || __instance.VoteInfo.ActiveVotes.Count > 0)
                {
                    args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.fail.voteinprogress"), Color.Red, args.SenderGameUser));
                    return true;
                }

                if (!Voting.GameVoteKick.CanVoteKick)
                {
                    args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.fail.oncooldown"), Color.Red, args.SenderGameUser));
                    return true;
                }

                GameUser voteKickUserToKick = __instance.GetGameUserByStringInput(args.SourceParameters);
                if (voteKickUserToKick == null || voteKickUserToKick.IsDisposed || voteKickUserToKick.IsModerator || voteKickUserToKick.IsBot)
                {
                    args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.votekick.fail.invaliduser"), Color.Red, args.SenderGameUser));
                    return true;
                }

                string consoleMess = "Creating vote-kick from '{0}' ({1}) against '{2}' ({3})";
                ConsoleOutput.ShowMessage(ConsoleOutputType.Information, string.Format(consoleMess, args.SenderGameUser.GetProfileName(), args.SenderGameUser.AccountName, voteKickUserToKick.GetProfileName(), voteKickUserToKick.AccountName));

                Voting.GameVoteKick vote = new Voting.GameVoteKick(GameVote.GetNextVoteID(), voteKickUserToKick);
                vote.ValidRemoteUniqueIdentifiers.AddRange(server.GetConnectedUniqueIdentifiers((NetConnection x) => x.GameConnectionTag() != null && x.GameConnectionTag().FirstGameUser != null && x.GameConnectionTag().FirstGameUser.UserIdentifier != voteKickUserToKick.UserIdentifier && x.GameConnectionTag().FirstGameUser.CanVote));
                __instance.VoteInfo.AddVote(vote);
                server.SendMessage(MessageType.GameVote, new Pair<GameVote, bool>(vote, false));
                server.SendMessage(MessageType.Sound, new NetMessage.Sound.Data("PlayerLeave", true, Vector2.Zero, 1f));

                string mess = LanguageHelper.GetText("sfdct.command.votekick.message", args.SenderGameUser.GetProfileName(), args.SenderGameUser.AccountName, voteKickUserToKick.GetProfileName(), voteKickUserToKick.AccountName);
                string mess2 = LanguageHelper.GetText("sfdct.command.votekick.message.victim", args.SenderGameUser.GetProfileName(), args.SenderGameUser.AccountName);
                string mess3 = LanguageHelper.GetText("sfdct.command.votekick.message.owner", voteKickUserToKick.GetProfileName(), voteKickUserToKick.AccountName);

                args.Feedback.Add(new(args.SenderGameUser, mess, Voting.GameVoteKick.PRIMARY_MESSAGE_COLOR));
                args.Feedback.Add(new(args.SenderGameUser, mess2, Voting.GameVoteKick.SECONDARY_MESSAGE_COLOR, voteKickUserToKick));
                args.Feedback.Add(new(args.SenderGameUser, mess3, Voting.GameVoteKick.SECONDARY_MESSAGE_COLOR, args.SenderGameUser));
            }
        }

        if (args.IsCommand("JOIN"))
        {
            if (!args.SenderGameUser.IsModerator && Settings.Get<bool>(SettingKey.SpectatorsOnlyModerators))
            {
                return true;
            }

            GameConnectionTag connectionTag = args.SenderGameUser.GetGameConnectionTag();

            if (connectionTag != null)
            {
                if (connectionTag.GameUsers.Count() > 1)
                {
                    args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.join.fail.localplayer"), Color.Red, args.SenderGameUser, null));
                    return true;
                }
                if (connectionTag.FirstGameUser == null || connectionTag.FirstGameUser.IsDisposed || !connectionTag.FirstGameUser.JoinedAsSpectator)
                {
                    return true;
                }

                GameSlot oldGameSlot = connectionTag.FirstGameUser.GameSlot;
                List<GameSlot> gameSlots = server.FindOpenGameSlots(server.GameInfo.DropInMode, connectionTag.GameUsers.Count(), server.GameInfo.EvenTeams, null);

                if (gameSlots != null && gameSlots.Count > 0)
                {
                    GameSlot gameSlot = gameSlots.First();
                    gameSlot.ClearGameUser(__instance);
                    gameSlot.GameUser = connectionTag.FirstGameUser;
                    gameSlot.CurrentState = GameSlot.State.Occupied;
                    connectionTag.FirstGameUser.GameSlot = gameSlot;
                    connectionTag.FirstGameUser.JoinedAsSpectator = false;

                    List<string> messArgs = [];
                    string mess = "menu.lobby.newPlayerJoined";
                    Color messColor = Constants.COLORS.PLAYER_CONNECTED;

                    messArgs.Add(connectionTag.FirstGameUser.GetProfileName());
                    if (gameSlot.CurrentTeam != Team.Independent)
                    {
                        mess = "menu.lobby.newPlayerJoinedTeam";
                    }

                    server.SendMessage(MessageType.ChatMessageSuppressDSForm, new NetMessage.ChatMessage.Data(mess, messColor, messArgs.ToArray()), null, null);
                    server.SendMessage(MessageType.Sound, new NetMessage.Sound.Data("PlayerJoin", true, Vector2.Zero, 1f), null);
                    server.SyncGameSlotInfo(gameSlot);
                    server.SyncGameUserInfo(connectionTag.FirstGameUser, null);

                    args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.join.message"), Color.Gray, args.SenderGameUser, null));
                }
                else
                {
                    args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.join.fail.nogameslot"), Color.Red, args.SenderGameUser, null));
                }

                return true;
            }
        }

        if (args.IsCommand("SPECTATE"))
        {
            if (!args.SenderGameUser.IsModerator && Settings.Get<bool>(SettingKey.SpectatorsOnlyModerators))
            {
                return true;
            }
            if (__instance.GetSpectatingUsers().Count >= Settings.Get<int>(SettingKey.SpectatorsMaximum))
            {
                return true;
            }

            GameConnectionTag connectionTag = args.SenderGameUser.GetGameConnectionTag();

            if (connectionTag != null)
            {
                if (connectionTag.GameUsers.Count() > 1)
                {
                    args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.spectate.fail.localplayer"), Color.Red, args.SenderGameUser, null));
                    return true;
                }
                if (connectionTag.FirstGameUser != null && !connectionTag.FirstGameUser.IsDisposed && connectionTag.FirstGameUser.JoinedAsSpectator)
                {
                    return true;
                }

                GameSlot gameSlot = connectionTag.FirstGameUser.GameSlot;
                gameSlot.GameUser = null;
                gameSlot.ClearGameUser(null);
                gameSlot.CurrentState = GameSlot.State.Open;
                connectionTag.FirstGameUser.GameSlot = null;
                connectionTag.FirstGameUser.JoinedAsSpectator = true;

                if (__instance.GameWorld != null)
                {
                    Player senderGamePlayer = __instance.GameWorld.GetPlayerByUserIdentifier(connectionTag.FirstGameUser.UserIdentifier);
                    senderGamePlayer?.SetUser(0);
                    senderGamePlayer?.Kill();
                }

                string mess = "menu.lobby.newPlayerJoinedTeam";
                string[] messArgs = [args.SenderGameUser.GetProfileName(), LanguageHelper.GetText("general.spectator")];
                Color messColor = Color.LightGray;

                server.SendMessage(MessageType.ChatMessageSuppressDSForm, new NetMessage.ChatMessage.Data(mess, messColor, messArgs), null, null);
                server.SendMessage(MessageType.Sound, new NetMessage.Sound.Data("PlayerLeave", true, Vector2.Zero, 1f), null);
                server.SyncGameSlotInfo(gameSlot, null);
                server.SyncGameUserInfo(connectionTag.FirstGameUser, null);

                args.Feedback.Add(new(args.SenderGameUser, LanguageHelper.GetText("sfdct.command.spectate.message.info"), Color.Gray, args.SenderGameUser, null));
                return true;
            }
        }

        return false;
    }
}