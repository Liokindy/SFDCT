using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SFDCT.Sync;
using SFD;
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
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, string.Format(mess, gameUser.GameSlotIndex, gameUser.GetProfileName(), gameUser.AccountName, status), messColor, args.SenderGameUser));

                    gameUserCount++;
                }
            }

            args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, string.Format(footer, gameUserCount), Color.LightBlue, args.SenderGameUser));

            return true;
        }

        // Client-only commands (no offline)
        if (__instance.GameOwner == GameOwnerEnum.Client)
        {
            if (args.IsCommand("CLIENTMOUSE"))
            {
                WorldHandler.ClientMouse = !WorldHandler.ClientMouse;

                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, string.Format("Client Mouse set to {0}", WorldHandler.ClientMouse), Color.LightBlue, args.SenderGameUser));
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
            // Server-only commands (no offline)
            if (__instance.GameOwner == GameOwnerEnum.Server)
            {
                // Enables debug functions of the editor, i.e
                // Mouse-dragging, mouse-deletion, etc.
                if (args.IsCommand("MOUSE", "SERVERMOUSE"))
                {
                    string mess = "Server-Mouse set to {0}";

                    WorldHandler.ServerMouse = !WorldHandler.ServerMouse;
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, string.Format(mess, WorldHandler.ServerMouse), null, null));

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
                // None
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
        if (args.IsCommand("JOIN"))
        {
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