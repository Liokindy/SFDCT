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
            string user = "- {0}: '{1}' ({2}) {3}";
            string bot = "- {0}: '{1}' {3}";
            string footer = "Found {0} users";

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

            args.Feedback.Add(new(args.SenderGameUser, string.Format(footer, gameUserCount), Color.LightBlue, args.SenderGameUser));

            return true;
        }

        // Client-only commands (no offline)
        if (__instance.GameOwner == GameOwnerEnum.Client)
        {
            if (args.IsCommand("CLIENTMOUSE"))
            {
                WorldHandler.ClientMouse = !WorldHandler.ClientMouse;

                args.Feedback.Add(new(args.SenderGameUser, string.Format("Client Mouse set to {0}", WorldHandler.ClientMouse), Color.LightBlue, args.SenderGameUser));
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
                string mess = "Server-Mouse moderators set to {0}";
                WorldHandler.ServerMouseNoModerators = !WorldHandler.ServerMouseNoModerators;

                args.Feedback.Add(new(args.SenderGameUser, string.Format(mess, WorldHandler.ServerMouseNoModerators)));
            }

            Color c1 = new Color(159, 255, 64);
            if (args.IsCommand("ADDMODCOMMANDS"))
            {
                args.Feedback.Add(new(args.SenderGameUser, "Adding moderator commands...", c1, args.SenderGameUser));

                for (int i = 0; i < args.Parameters.Count; i++)
                {
                    string modCommand = args.Parameters[i].ToUpperInvariant();

                    args.Feedback.Add(new(args.SenderGameUser, $"- Added '{modCommand}'", c1 * 0.5f, args.SenderGameUser));
                    Constants.MODDERATOR_COMMANDS.Add(modCommand);
                }
                args.Feedback.Add(new(args.SenderGameUser, $"Added '{args.Parameters.Count}' moderator commands", c1 * 0.75f, args.SenderGameUser));

                SFDConfig.SaveConfig(SFDConfigSaveMode.HostGameOptions);
                return true;
            }

            if (args.IsCommand("REMOVEMODCOMMANDS") && args.Parameters.Count > 0)
            {
                args.Feedback.Add(new(args.SenderGameUser, "Removing moderator commands...", c1, args.SenderGameUser));

                for (int i = 0; i < args.Parameters.Count; i++)
                {
                    string modCommand = args.Parameters[i];

                    if (Constants.MODDERATOR_COMMANDS.Contains(modCommand))
                    {
                        Constants.MODDERATOR_COMMANDS.Remove(modCommand);
                        args.Feedback.Add(new(args.SenderGameUser, $"- Removed '{modCommand}'", c1 * 0.5f, args.SenderGameUser));
                    }
                    if (Constants.MODDERATOR_COMMANDS.Contains(modCommand.ToUpperInvariant()))
                    {
                        Constants.MODDERATOR_COMMANDS.Remove(modCommand.ToUpperInvariant());
                        args.Feedback.Add(new(args.SenderGameUser, $"- Removed '{modCommand.ToUpperInvariant()}'", c1 * 0.5f, args.SenderGameUser));
                    }

                    Constants.MODDERATOR_COMMANDS.Add(modCommand);
                }
                args.Feedback.Add(new(args.SenderGameUser, $"Removed '{args.Parameters.Count}' moderator commands", c1 * 0.75f, args.SenderGameUser));

                SFDConfig.SaveConfig(SFDConfigSaveMode.HostGameOptions);
                return true;
            }

            if (args.IsCommand("CLEARMODCOMMANDS"))
            {
                args.Feedback.Add(new(args.SenderGameUser, "Clearing all moderator commands...", c1, args.SenderGameUser));
                int count = Constants.MODDERATOR_COMMANDS.Count;
                Constants.MODDERATOR_COMMANDS.Clear();

                args.Feedback.Add(new(args.SenderGameUser, $"Cleared {count} moderator commands.", c1 * 0.75f, args.SenderGameUser));

                SFDConfig.SaveConfig(SFDConfigSaveMode.HostGameOptions);
                return true;
            }

            if (args.IsCommand("LISTMODCOMMANDS"))
            {
                args.Feedback.Add(new(args.SenderGameUser, "Listing all moderator commands...", c1, args.SenderGameUser));

                if (Constants.MODDERATOR_COMMANDS.Count > 0)
                {
                    for (int i = 0; i < Constants.MODDERATOR_COMMANDS.Count; i++)
                    {
                        string modCommand = Constants.MODDERATOR_COMMANDS[i];
                        args.Feedback.Add(new(args.SenderGameUser, $"- '{modCommand}'", c1 * 0.5f, args.SenderGameUser));
                    }
                    args.Feedback.Add(new(args.SenderGameUser, string.Format("Moderators can use {0} command(s)", Constants.MODDERATOR_COMMANDS.Count), c1 * 0.8f, args.SenderGameUser));
                }
                else
                {
                    args.Feedback.Add(new(args.SenderGameUser, "- Moderators can use ALL commands!", c1 * 0.75f, args.SenderGameUser));
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
                    string mess = "Server-Mouse set to {0}";

                    WorldHandler.ServerMouse = !WorldHandler.ServerMouse;
                    args.Feedback.Add(new(args.SenderGameUser, string.Format(mess, WorldHandler.ServerMouse), null, null));

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
                            args.Feedback.Add(new(args.SenderGameUser, "Failed to parse gravity Y", args.SenderGameUser));
                            return true;
                        }
                    }

                    if (args.Parameters.Count == 2)
                    {
                        if (!float.TryParse(args.Parameters[0].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out gx))
                        {
                            args.Feedback.Add(new(args.SenderGameUser, "Failed to parse gravity X", args.SenderGameUser));
                            return true;
                        }

                        if (!float.TryParse(args.Parameters[1].Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out gy))
                        {
                            args.Feedback.Add(new(args.SenderGameUser, "Failed to parse gravity Y", args.SenderGameUser));
                            return true;
                        }
                    }

                    string mess = "World Gravity set to {0}";
                    Vector2 gravityVector = new Vector2(gx, gy);

                    __instance.GameWorld.GetActiveWorld.Gravity = gravityVector;
                    __instance.GameWorld.GetBackgroundWorld.Gravity = gravityVector;

                    args.Feedback.Add(new(args.SenderGameUser, string.Format(mess, gravityVector.ToString())));
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

                    string mess = "Dealt {0} damage to '{1}'";
                    GameUser user = __instance.GetGameUserByStringInput(args.Parameters[0], args.SenderGameUser);
                    if (user != null && !user.IsDisposed)
                    {
                        Player userPlayer = __instance.GameWorld.GetPlayerByUserIdentifier(user.UserIdentifier);
                        if (userPlayer != null && !userPlayer.IsDisposed)
                        {
                            userPlayer.TakeMiscDamage(damage, false);
                            args.Feedback.Add(new(args.SenderGameUser, string.Format(mess, damage, user.GetProfileName())));
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
                    GameConnectionTag gameConnectionTag = gameUser.GetGameConnectionTag();
                    if (gameUser != null && !gameUser.IsDisposed && gameConnectionTag != null && !gameConnectionTag.IsDisposed)
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

                        string msg = "Server-movement of {0} set to {1}";
                        args.Feedback.Add(new(args.SenderGameUser, string.Format(msg, gameUser.AccountName, resetServerMovement ? "default" : useServerMovement), args.SenderGameUser));

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

        // Public commands
        if (__instance.GameOwner == GameOwnerEnum.Server)
        {
            if (args.IsCommand("VOTEKICK"))
            {
                if (args.Parameters.Count <= 0)
                {
                    args.Feedback.Add(new(args.SenderGameUser, "You need to specify a user to kick", Color.Red, args.SenderGameUser));
                    return true;
                }

                if (__instance.VoteInfo == null || __instance.VoteInfo.ActiveVotes.Count > 0)
                {
                    args.Feedback.Add(new(args.SenderGameUser, "There is already a vote in progress", Color.Red, args.SenderGameUser));
                    return true;
                }

                if (!Voting.GameVoteKick.CanVoteKick)
                {
                    args.Feedback.Add(new(args.SenderGameUser, "You can't start another vote-kick right now", Color.Red, args.SenderGameUser));
                    return true;
                }

                GameUser voteKickUserToKick = __instance.GetGameUserByStringInput(args.SourceParameters);
                if (voteKickUserToKick == null || voteKickUserToKick.IsDisposed || voteKickUserToKick.IsModerator || voteKickUserToKick.IsBot)
                {
                    args.Feedback.Add(new(args.SenderGameUser, "You can't start a vote-kick against this user", Color.Red, args.SenderGameUser));
                    return true;
                }

                string consoleMess = "Creating vote-kick from '{0}' ({1}) against '{2}' ({3})";
                ConsoleOutput.ShowMessage(ConsoleOutputType.Information, string.Format(consoleMess, args.SenderGameUser.GetProfileName(), args.SenderGameUser.AccountName, voteKickUserToKick.GetProfileName(), voteKickUserToKick.AccountName));

                Voting.GameVoteKick vote = new Voting.GameVoteKick(GameVote.GetNextVoteID(), voteKickUserToKick);
                vote.ValidRemoteUniqueIdentifiers.AddRange(server.GetConnectedUniqueIdentifiers((NetConnection x) => x.GameConnectionTag() != null && x.GameConnectionTag().FirstGameUser != null && x.GameConnectionTag().FirstGameUser.UserIdentifier != voteKickUserToKick.UserIdentifier && x.GameConnectionTag().FirstGameUser.CanVote));
                __instance.VoteInfo.AddVote(vote);
                server.SendMessage(MessageType.GameVote, new Pair<GameVote, bool>(vote, false));
                server.SendMessage(MessageType.Sound, new NetMessage.Sound.Data("PlayerLeave", true, Vector2.Zero, 1f));

                string mess = "'{0}' ({1}) HAS STARTED A VOTE-KICK AGAINST '{2}' ({3})";
                string mess2 = "- '{0}' ({1}) has started a vote-kick against you";
                string mess3 = "- You started a vote-kick against '{0}' ({1})";

                args.Feedback.Add(new(args.SenderGameUser, string.Format(mess, args.SenderGameUser.GetProfileName(), args.SenderGameUser.AccountName, voteKickUserToKick.GetProfileName(), voteKickUserToKick.AccountName), Voting.GameVoteKick.PRIMARY_MESSAGE_COLOR));
                args.Feedback.Add(new(args.SenderGameUser, string.Format(mess2, args.SenderGameUser.GetProfileName(), args.SenderGameUser.AccountName), Voting.GameVoteKick.SECONDARY_MESSAGE_COLOR, voteKickUserToKick));
                args.Feedback.Add(new(args.SenderGameUser, string.Format(mess3, voteKickUserToKick.GetProfileName(), voteKickUserToKick.AccountName), Voting.GameVoteKick.SECONDARY_MESSAGE_COLOR, args.SenderGameUser));
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
                    args.Feedback.Add(new(args.SenderGameUser, "You can't join back with more than 1 local player", Color.Red, args.SenderGameUser, null));
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

                    args.Feedback.Add(new(args.SenderGameUser, "You stopped spectating and joined back", Color.Gray, args.SenderGameUser, null));
                }
                else
                {
                    args.Feedback.Add(new(args.SenderGameUser, "You can't join back, unavailable game-slot", Color.Red, args.SenderGameUser, null));
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
                    args.Feedback.Add(new(args.SenderGameUser, "You can't spectate with more than 1 local player", Color.Red, args.SenderGameUser, null));
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

                args.Feedback.Add(new(args.SenderGameUser, "Use /JOIN to stop spectating and join an available game-slot", Color.Gray, args.SenderGameUser, null));
                return true;
            }
        }

        return false;
    }
}