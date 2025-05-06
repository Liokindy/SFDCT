using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
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

                Color messColor = Color.LightBlue * (gameUser.UserIdentifier % 2 == 0 ? 0.5f : 0.6f);
                string mess = string.Format(user, gameUser.GameSlotIndex, gameUser.GetProfileName(), gameUser.AccountName, status);

                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess, messColor, args.SenderGameUser));
            }

            args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, string.Format(footer, gameUsers.Count()), Color.LightBlue, args.SenderGameUser));

            return true;
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
            // Enables debug functions of the editor, i.e
            // Mouse-dragging, mouse-deletion, etc.
            if (args.IsCommand("EDITORDEBUG"))
            {
                string mess = "Editor-Debug: {0}";
                string mess2 = "(o . o)";

                bool enabled = !WorldHandler.EditorDebug;
                if (args.Parameters.Count >= 1)
                {
                    if (args.Parameters[0] == "1" || args.Parameters[0].Equals("TRUE", StringComparison.OrdinalIgnoreCase))
                    {
                        enabled = true;
                    }
                    if (args.Parameters[0] == "0" || args.Parameters[0].Equals("FALSE", StringComparison.OrdinalIgnoreCase))
                    {
                        enabled = false;
                    }
                }
                WorldHandler.EditorDebug = enabled;

                args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, string.Format(mess, enabled), args.SenderGameUser, null));
                if (enabled)
                {
                    args.Feedback.Add(new ProcessCommandMessage(args.SenderGameUser, mess2, Color.IndianRed, null, args.SenderGameUser));
                }
                
                return true;
            }
        }

        // Moderator commands
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
            }
        }

        return false;
    }
}