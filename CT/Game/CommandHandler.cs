using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using SFDCT.Helper;
using SFDCT.Sync;
using SFD;
using SFD.Core;
using SFD.States;
using SFD.Voting;
using SFD.Objects;
using SFD.Weapons;
using Lidgren.Network;
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
            string header = "Listing all users in the lobby...";
            string user = "- {0}: '{1}' ({2}) {3}";
            string footer = "Found {0} players";

            args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, header, Color.LightBlue, args.SenderGameUser));

            IEnumerable<GameUser> gameUsers = __instance.GetGameUsers();
            foreach (GameUser gameUser in gameUsers)
            {
                string status = "";

                if (gameUser.JoinedAsSpectator) { status = "SPECTATOR"; }
                if (gameUser.SpectatingWhileWaitingToPlay) { status = "WAITING"; }
                if (gameUser.IsModerator) { status = "MODERATOR"; }
                if (gameUser.IsHost) { status = "HOST"; }

                Color messColor = Color.LightBlue * (gameUser.UserIdentifier % 2 == 0 ? 0.8f : 0.9f);
                string mess = string.Format(user, gameUser.GameSlotIndex, gameUser.GetProfileName(), gameUser.AccountName, status);

                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess, messColor, args.SenderGameUser));
            }

            args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, string.Format(footer, gameUsers.Count()), Color.LightBlue, args.SenderGameUser));

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
                // None
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

        return false;
    }
}